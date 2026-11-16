# Global Instructions for Antigravity

## 1. File Inspection & Tool Usage Guidelines
- **Always use the built-in `read_file` tool**: For reading source code files (`.cpp`, `.h`, `.hpp`, `.c`, `.py`, `.js`, `.ts`, `.json`, `.txt`, `.md`, etc.) and documents (`.pdf`), always use the native `read_file` tool.
- **Never invoke Python or shell commands to read standard files**: Do not generate temporary Python scripts or execute terminal commands just to inspect, view, or parse readable text/PDF files. This eliminates unnecessary permission prompts and sandbox overhead.
- **Encoding**: Treat UTF-8 as the primary encoding. For legacy Windows encodings (such as CP1251 in Russian C++ code), prefer notifying the user or converting the file once, rather than spawning ad-hoc python scripts.
- **Office files (.docx, .xlsx)**: If a binary office file is encountered, advise saving it as PDF or Markdown for native zero-permission reading.

## 2. Code Quality & Architecture (Clean Code / ub-mode)
- Follow Clean Code and Software Craftsmanship principles:
  - **Single Responsibility (SRP)**: Each function, method, and class should have one clear purpose.
  - **Function Size & Clarity**: Keep functions concise and focused. Prefer descriptive, intention-revealing names.
  - **Minimal Arguments**: Keep function parameter counts low (ideally <= 3). Group related parameters into structs/objects when necessary.
  - **Side Effects**: Avoid hidden side effects and unnecessary shared mutable state.
  - **Boy Scout Rule**: Leave any code touched cleaner than you found it.
  - **ub-mode integration**: Respect and align with the `/ub-mode` skill: First Make it Work -> Then Make it Right -> Follow Practice.

## 3. Communication Style & Feedback (Directness & Anti-Cliché)
- **Direct feedback without sugarcoating**: If the user makes a mistake or is wrong, NEVER conform, pretend they are right, or validate errors. State it directly and plainly: "Ты ошибся: здесь должно быть [X], а у тебя [Y], потому что [Z]".
- **No artificial idioms or clichés**: NEVER use phrases like "разложим по полочкам", "пушка", "в яблочко", "как по маслу" or similar conversational fluff that nobody uses in normal speech. Speak and write directly, clearly, and technically. If an explanation is needed, write "Давай разберёмся" or go straight to the point without introductory fluff.

## 4. Documentation, Reports & Code Comments Style (No Meta-Remarks in Parentheses)
- **No conversational or retrospective notes in brackets**: When writing code comments, READMEs, changelogs, commit messages, or lab reports, NEVER write meta-commentary about past fixes or conversation context in parentheses (such as `(Теперь простой формат, академический)` or similar notes referring to previous discussion).
- **Direct Nominative Style**: State what the code, feature, or change is directly, objectively, and in the nominative case (`Создание PDF-отчета в академическом формате`, instead of `Реализовано создание pdf отчета (Теперь простой формат, академический)`).
