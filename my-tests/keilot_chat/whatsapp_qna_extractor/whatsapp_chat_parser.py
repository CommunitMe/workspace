""" example usage:

from chat_parser import parse_chat_file
chat_df = parse_chat_file("_chat.txt")
"""

import re
import pandas as pd
from typing import List, Dict
from datetime import datetime
import time

def clean_invisible_chars(text: str) -> str:
    invisible_chars = [
        "\u200e", "\u202a", "\u202b", "\u202c", "\u202d", "\u202e",
        "\u202f", "\ufeff"
    ]
    for char in invisible_chars:
        text = text.replace(char, "")
    return text

def parse_chat_file(file_path: str) -> pd.DataFrame:
    start_time = time.time()
    with open(file_path, encoding="utf-8") as f:
        raw_lines = f.readlines()

    clean_lines = (clean_invisible_chars(line.strip()) for line in raw_lines if line.strip())

    message_start_pattern = re.compile(
        r"^\[(\d{1,2}[./]\d{1,2}[./]\d{4}), (\d{1,2}:\d{2})(?::(\d{2}))?\] (.*?): (.*)"
    )

    messages: List[Dict[str, str]] = []
    current_message = {}
    total_lines = 0
    skipped_lines = 0

    for i, line in enumerate(clean_lines):
        total_lines += 1
        match = message_start_pattern.match(line)
        if match:
            if current_message:
                messages.append(current_message)
            try:
                time_str = match.group(1).replace(".", "/") + " " + match.group(2)
                if match.group(3):
                    time_str += ":" + match.group(3)
                timestamp = pd.to_datetime(time_str, dayfirst=True, errors="coerce")
                current_message = {
                    "date": timestamp.date().isoformat(),
                    "timestamp": timestamp,
                    "sender": match.group(4),
                    "message": match.group(5),
                }
            except Exception as e:
                print(f"❌ Error on line {i}: {e}")
                skipped_lines += 1
                current_message = {}
        else:
            if current_message:
                current_message["message"] += "\n" + line
            else:
                skipped_lines += 1

    if current_message:
        messages.append(current_message)

    chat_df = pd.DataFrame(messages)
    chat_df = chat_df.dropna(subset=["timestamp"])

    duration = time.time() - start_time
    print(f"✅ Parsing complete.\n- Total lines: {total_lines}\n- Messages parsed: {len(chat_df)}\n- Skipped lines: {skipped_lines}\n- Duration: {duration:.2f} seconds")

    return chat_df
