; Find the one's compliment (inverse) of a number

	TTL	Ch4Ex2 - invert
	AREA	Program, CODE, READONLY	
	ENTRY
	
Main			
	LDR	R1,Value		; Load the number to be complimented
	MVN	R1,R1			; NOT the contents of R1
	STR	R1,Result		; Store the result
	SWI	&11

Value	DCD	&C123			; Value to be complemented
Result	DCD	0			; Storage for result

	END