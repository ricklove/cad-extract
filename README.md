# Cad Extract

Extract Tabular Data from Cad Drawing Files

## Features

- [x] Import .dxf files
- [ ] Import .pdf files
- [x] Preview Imported Files
- [x] Extract tables from cad drawings
    - [x] Identify Table Cells
    - [x] Locate Text within a Table Cell
    - [x] Identify Merged Cells
    - [x] Identify Tables (not touching)
    - [ ] Partition Separate Tables (even if touching)
- [x] Preview Extracted Tables
- [ ] Generate a .cvs file (without merged cells)
- [ ] Generate a .xlsx file (with merged cells)
- [ ] Transform extracted rows into a .json file (converting column values into object fields)
- [ ] Verify Extraction Accuracy with Visual Round Trip Automatic Comparison
    - (Requires a pdf version of the source document even if extraction is from dxf)
    - Extract the tables
    - Recreate a pdf from the extracted data (only table lines and text)
    - (Optional) Visually compare any differences (missing text, misplaced text, etc.)

## Bugs

- [x] Cell Column and Row should work for multiple neighbor connections

## License

MIT