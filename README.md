# Cad Extract

Extract Tabular Data from Cad Drawing Files

## Features

- [ ] Extract tables from cad drawings
- [x] Import .dxf files
- [ ] Import .pdf files
- [ ] Generate a .cvs file (without merged cells)
- [ ] Generate a .xsls file (with merged cells)
- [ ] Transform extracted rows into a .json file (converting column values into object fields)
- [ ] Verify Extraction Accuracy with Visual Round Trip Automatic Comparison
    - (Requires a pdf version of the source document even if extraction is from dxf)
    - Extract the tables
    - Recreate a pdf from the extracted data (only table lines and text)
    - (Optional) Visually compare any differences (missing text, misplaced text, etc.)

## License

MIT