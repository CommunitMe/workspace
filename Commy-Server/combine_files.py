import os

# Folder where your files are located
folder_path = "./"
output_file = "combined_output.txt"

# Open the output file in write mode
with open(output_file, 'w', encoding='utf-8') as outfile:
    # Walk through the folder and its subfolders
    for root, dirs, files in os.walk(folder_path):
        for file_name in files:
            file_path = os.path.join(root, file_name)
            try:
                # Write the file name as a header
                outfile.write(f"\n--- File: {file_name} ---\n")
                # Read and write the content of the file
                with open(file_path, 'r', encoding='utf-8') as infile:
                    content = infile.read()
                    outfile.write(content + "\n")
            except Exception as e:
                print(f"Error processing {file_path}: {e}")

print(f"All files combined into {output_file}")
