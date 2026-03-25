import csv
import json
import os

def convert_csv_to_json(csv_file_path, json_file_path, community_name, whatsapp_group_name):
    """
    Converts a CSV file to a JSON file matching the WhatsappQnAItem interface.
    """
    
    # Check if file exists
    if not os.path.exists(csv_file_path):
        print(f"Error: The file {csv_file_path} was not found.")
        return

    json_data = []

    try:
        with open(csv_file_path, mode='r', encoding='utf-8-sig') as csv_file:
            # Use DictReader to automatically map header names to values
            csv_reader = csv.DictReader(csv_file)
            
            for row in csv_reader:
                # פיצול השמות לרשימה וניקוי רווחים
                askers = [name.strip() for name in row.get('askers_name', '').split(',') if name.strip()]
                answerers = [name.strip() for name in row.get('answerers_name', '').split(',') if name.strip()]

                item = {
                    "question": row.get('question', '').strip(),
                    "answer": row.get('answer', '').strip(),
                    "askers_name": askers,      # עכשיו זה רשימה ['name1', 'name2']
                    "answerers_name": answerers, # עכשיו זה רשימה
                    "date": row.get('date', '').strip(),
                    "community_name": community_name,
                    "whatsapp_group_name": whatsapp_group_name
                }
                
                # Only add if there is actual content
                if item["question"] and item["answer"]:
                    json_data.append(item)

        # Write to JSON file
        with open(json_file_path, mode='w', encoding='utf-8') as json_file:
            # ensure_ascii=False is CRITICAL for Hebrew to look like Hebrew (not \u05d0...)
            json.dump(json_data, json_file, indent=2, ensure_ascii=False)

        print(f"Success! Converted {len(json_data)} items.")
        print(f"Saved to: {json_file_path}")

    except Exception as e:
        print(f"An error occurred: {e}")

# --- Configuration ---
# 1. Name of your input CSV file
INPUT_CSV = 'data\\whatsapp_chat\\retamim\\Womens_stuff.csv' 

# 2. Name of your output JSON file
OUTPUT_JSON = 'data\\whatsapp_chat\\retamim\\Womens_stuff.json'

# 3. Data to inject (Since these aren't in the CSV, define them here)
COMMUNITY_NAME = "retamim"  # Example ID, change to your specific number
GROUP_NAME = "Women's Stuff"  # Example name, change to your specific group name

# --- Run the conversion ---
if __name__ == "__main__":
    convert_csv_to_json(INPUT_CSV, OUTPUT_JSON, COMMUNITY_NAME, GROUP_NAME)