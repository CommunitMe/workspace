import pandas as pd
from typing import List
from langchain_openai import ChatOpenAI
from langchain_core.prompts import ChatPromptTemplate
from pydantic import BaseModel, Field

# --- הגדרת המבנה ---
class QAPair(BaseModel):
    question: str = Field(description="השאלה שעלתה")
    answer: str = Field(description="התשובה, כולל ציון שמות המשיבים בתוך הטקסט")
    askers_name: List[str] = Field(description="רשימת שמות של מי ששאל את השאלה")
    answerers_name: List[str] = Field(description="רשימת שמות של מי שענה על השאלה")
    date: str = Field(description="התאריך שבו נשאלה השאלה")

class ExtractedKnowledge(BaseModel):
    qa_pairs: List[QAPair]

# --- הפונקציה המעודכנת ---
def process_df_with_overlap(df, chunk_size=100, overlap=20):
    # 1. שימוש ב-gpt-4o-mini שהוא מעולה לזה וזול
    llm = ChatOpenAI(model="gpt-4o-mini", temperature=0)
    
    # 2. שימוש ב-Structured Output (זה השינוי הגדול!)
    # זה גורם ל-LangChain להשתמש ב-API של OpenAI שמחזיר אובייקט ולא טקסט
    structured_llm = llm.with_structured_output(ExtractedKnowledge)

    # 3. עדכון הפרומפט - הורדתי את {format_instructions} כי זה כבר לא נדרש
    prompt = ChatPromptTemplate.from_template("""
    אתה מנהל ידע בכיר ומנוסה בארגון.
    פעל כמנהל ידע. המטרה שלך היא לחלץ מתוך יומן השיחה הזה מאגר שאלות ותשובות שיהיו רלוונטיות למשתמשים גם בעתיד.

    הנחיות לסינון (חשוב מאוד):

    ערך לטווח ארוך: חלץ רק מידע שהוא 'ידע כללי', נהלים, פתרונות טכניים, המלצות או מידע על זכויות. 
    התעלם ממידע חולף: אל תחלץ שאלות על נושאים רגעיים (כמו 'מי יוצא למסעדה היום?', 'יש טרמפ?', 'מי בא לאכול?')
    תשובה ברורה: חלץ רק אם ניתנה תשובה מלאה ומועילה. אם אין תשובה ברורה, אל תחלץ את הזוג.
    לפני שאתה מחלץ זוג, וודא ב-200% שההודעה שזיהית כ"תשובה" אכן מגיבה לשאלה הספציפית הזו. שלא יהיו כאן טעויות! ממש אסור להביא שאלות ותשובות לא תואמות.


    הנחיות לעריכה ועיצוב:
    עריכה קלה: שמור על שפת המקור, אך אם השאלה או התשובה כתובות בסלנג לא ברור או בהודעות קטועות חבר אותן למשפט אחד ברור ותקני בעברית.
    הקשר: אם השאלה מסתמכת על הקשר קודם (למשל 'איך מגיעים לשם?'), החלף את המילה 'לשם' במקום המפורש המדובר.

    אם לא נמצא מידע העונה לקריטריונים, החזר רשימה ריקה.
    Chat Log:
    {chat_chunk}
    """)

    # השרשור החדש: פרומפט -> מודל מובנה (בלי פרסר חיצוני)
    chain = prompt | structured_llm

    all_qa_pairs = []
    step = chunk_size - overlap
    
    print(f"Starting processing with Structured Output...")

    for i in range(0, len(df), step):
        df_chunk = df.iloc[i : i + chunk_size]
        if len(df_chunk) < 3:
            continue

        text_chunk = "\n".join(
            f"[{row['date']}, {row['sender']}]: {row['message']}" 
            for _, row in df_chunk.iterrows()
        )

        try:
            # invoke מחזיר ישירות את האובייקט ExtractedKnowledge (לא צריך Parsing)
            result = chain.invoke({"chat_chunk": text_chunk})
            
            if result and result.qa_pairs:
                print(f"Batch {i}: Found {len(result.qa_pairs)} pairs.")
                all_qa_pairs.extend(result.qa_pairs)
            else:
                 print(f"Batch {i}: No pairs found.")

        except Exception as e:
            print(f"Error in batch starting at {i}: {e}")
            # במקרים נדירים של סירוב תוכן (Content Filter) זה יכול ליפול
            continue
            
    return all_qa_pairs