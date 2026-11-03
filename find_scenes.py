import os, re

pattern = re.compile(r'LoadScene\s*\(|"MainMenu"|"Main_Menu"|"LevelSelect"|"Gameplay"')

matches = []
for root, dirs, files in os.walk('DemHoiDenLong/Assets'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f).replace('\\', '/')
            with open(p, 'r', encoding='utf-8', errors='ignore') as file:
                for line_no, line in enumerate(file, 1):
                    if pattern.search(line):
                        matches.append(f"{p}:{line_no}: {line.strip()}")

for m in matches:
    print(m)
print(f"Total matches: {len(matches)}")
