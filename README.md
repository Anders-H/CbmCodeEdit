# Commodore BASIC Code Editor

A simple text editor for editing enhanced Commodore BASIC code, and exporting it as runnable source code.

## Editor features

- Load/save source files
- Multiple compile targets: VIC-20 (first version), Commodore 64, Commodore 128 40 columns, Commodore 128 80 columns
- Export runnable code

## Language extensions

- Labels instead of line numbers
- Long variable names
- Constant values
- Compound assignment

## Other features

- Check for declared variables
- Check that exported code lines not are longer than the maximum length
  - VIC-20: 88 characters
  - Commodore 64: 80 characters
  - Commodore 128: 160 characters

## Screenshot

![Screenshot](https://raw.githubusercontent.com/Anders-H/CbmCodeEdit/main/screenshot.jpg)

## Examples

### Hello world

*Input:*

```
:loop print "hello world"
goto loop
```

*Output:*

```
0 print "hello world"
1 goto 0
```

*Input:*

```
:loop
print "hello world"
goto loop
```

*Output:*

```
0 rem
1 print "hello world"
2 goto 0
```