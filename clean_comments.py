import os
import re

# Refined regex for C#
# 1. Verbatim strings @"..."
# 2. Regular strings "..."
# 3. Character literals '...'
# 4. Block comments /* ... */
# 5. Line comments // ...
pattern = re.compile(r'(@"(?:[^"]|"")*"|"(?:[^"\\\\]|\\\\.)*"|\'(?:[^\'\\\\]|\\\\.)*\'|/\*[\s\S]*?\*/|//.*)')

def clean_file(filepath):
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except:
        try:
            with open(filepath, 'r', encoding='latin-1') as f:
                content = f.read()
        except:
            return False

    def replacer(match):
        s = match.group(0)
        if s.startswith('/'):
            return ""
        return s

    cleaned = pattern.sub(replacer, content)
    
    # Clean up XML comments specifically if missed
    cleaned = re.sub(r'///.*', '', cleaned)
    
    # Post-process: remove trailing whitespace and collapse empty lines
    lines = [line.rstrip() for line in cleaned.splitlines()]
    final = "\n".join(lines)
    final = re.sub(r'\n{3,}', '\n\n', final)
    
    if final != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(final)
        return True
    return False

search_dir = r'c:\Users\Luk\Desktop\VITRUVIUS4.1\Assets'
count = 0
for root, dirs, files in os.walk(search_dir):
    if any(skip in root for skip in ['Library', 'Temp', '.git', 'Packages']):
        continue
    for file in files:
        if file.endswith('.cs'):
            if clean_file(os.path.join(root, file)):
                count += 1

# Also check root
for file in os.listdir(r'c:\Users\Luk\Desktop\VITRUVIUS4.1'):
    if file.endswith('.cs') and file != 'clean_comments.py' and file != 'syntax_check.cs':
        if clean_file(os.path.join(r'c:\Users\Luk\Desktop\VITRUVIUS4.1', file)):
            count += 1

print(f"Cleaned {count} files.")
