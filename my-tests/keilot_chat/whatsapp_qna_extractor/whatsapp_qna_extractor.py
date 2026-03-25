import pandas as pd
from chat_qa_extractor import process_df_with_overlap
from whatsapp_chat_parser import parse_chat_file

def whatsapp_qna_extractor(chat_file_path: str, output_csv_path: str):
    whatsapp_chat_data = parse_chat_file(chat_file_path)
    extracted_data = process_df_with_overlap(whatsapp_chat_data, chunk_size=200, overlap=20)

    data = [item.model_dump() for item in extracted_data] 
    df = pd.DataFrame(data)
    df['askers_name'] = df['askers_name'].apply(lambda x: ", ".join(x) if isinstance(x, list) else x)
    df['answerers_name'] = df['answerers_name'].apply(lambda x: ", ".join(x) if isinstance(x, list) else x)
    df.to_csv(output_csv_path, index=False, encoding="utf-8-sig")

# run the main
if __name__ == "__main__":
    chat_file_path = "data\\whatsapp_chat\\retamim\\Womens_stuff.txt"
    output_csv_path = "data\\whatsapp_chat\\retamim\\Womens_stuff.csv"
    whatsapp_qna_extractor(chat_file_path, output_csv_path)