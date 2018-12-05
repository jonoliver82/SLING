; Add two numbers and store the result

	TTL	Ch4Ex4 - add2
	AREA	Program, CODE, READONLY
	ENTRY

Main
	LDR	R0,=Value1	; Load the address of first value
	LDR	R1,[R0]		; Load what is at that address
	ADD	R0,R0,#0x4	; Adjust the pointer
	LDR	R2,[R0]		; Load what is at the new addr
	ADD	R1,R1,R2	; ADD together
	LDR	R0,=Result	; Load the storage address
	STR	R1,[R0]		; Store the result
	SWI	&11		; All done

Value1	DCD	&37E3C123	; First value
Value2	DCD	&367402AA	; Second value
Result	DCD	0		; Space to store result

	END