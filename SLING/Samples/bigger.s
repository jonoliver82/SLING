; Find the larger of two numbers

	TTL	Ch4Ex7 - bigger
	AREA	Program, CODE, READONLY
	ENTRY

Main
	LDR	R1,Value1		; Load the first value to be compared
	LDR	R2,Value2		; Load the second value to be compared
	CMP	R1,R2			; Compare them
	BHI	Done			; If R1 contains the highest
	MOV	R1,R2			; otherwise overwrite R1
Done
	STR	R1, Result		; Store the result
	SWI	&11

Value1	DCD	&12345678		; Value to be compared
Value2	DCD	&87654321		; Value to be compared
Result	DCD	0			; Space to store result

	END