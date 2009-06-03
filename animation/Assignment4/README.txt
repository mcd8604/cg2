Assignment 4 - Articulated Figures by Mike DeMauro

1. Running the application:
	
	Run the solution using Microsoft Visual Studio 2008

	CONTROLS:
		
		SPACE - Reset
		P - Pause / Unpause
		Up - Increase sampling rate
		Down - Decrease sampling rate
		Left - Decrement current frame
		Right - Increment current frame	

2. Building the application:
	
	Build the solution using Microsoft Visual Studio 2008. (XNA 3.0 is required)

3. Platform

	Microsoft Windows XP or Vista

4. Notes

	Appearance of the figure in motion is incorrect. In my attempts to debug this issue I have:

	- Verified correct tree structure in memory.
	- Verified correct node offset values.
	- Verified correct motion data parsing.
	- Attempted different orders of matrix multiplication.
	
	I left the current order of matrix multiplication as:

		yRotation * xRotation * zRotation * translation * stackTop

	Logically, this is correct to me, yet the character does not appear correct.