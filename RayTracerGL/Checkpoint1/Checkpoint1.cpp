// Checkpoint1.cpp : Defines the entry point for the console application.
// author: Mike DeMauro

#include "stdafx.h"

#include "GL/glut.h"

// Screen size
#define RES_WIDTH 800.0
#define RES_HEIGHT 600.0

// Holds values for the View transform
struct Camera {
	int ID;

	GLdouble eyeX;
	GLdouble eyeY;
	GLdouble eyeZ;

	GLdouble centerX;
	GLdouble centerY;
	GLdouble centerZ;

	GLdouble upX;
	GLdouble upY;
	GLdouble upZ;

	void CreateLookAt() {
		gluLookAt ( eyeX, eyeY, eyeZ,
			 centerX, centerY, centerZ,
			 upX, upY, upZ);
	}
};

// Camera 
Camera cam;

// Sets up lighting
void InitLighting() {
	glEnable(GL_LIGHTING);
	//glLightModeli( GL_LIGHT_MODEL_TWO_SIDE, GL_TRUE );

	glEnable(GL_LIGHT0);
    
	glEnable(GL_NORMALIZE);

	glEnable(GL_DEPTH_TEST);
}

//Initializes OpenGL
void Initialize() {
	// Init GL 
	glClearColor (0.4, 0.6, 1.0, 0.0);
	glShadeModel (GL_SMOOTH);

	// Init lighting
	InitLighting();

	// Init camera
	cam = *new Camera();
	
	cam.eyeX = 3.0;
	cam.eyeY = 4.0;
	cam.eyeZ = 15.0;

	cam.centerX = 3.0;
	cam.centerY = 0.0;
	cam.centerZ = -70.0;

	cam.upY = 1.0;
	cam.ID = 0;
	
	// clear the matrix
	glLoadIdentity ();    
}

// lighting parameters
GLfloat position[] = { 5.0, 8.0, 15.0, 1.0 };
GLfloat diffuse[] = { 1.0, 1.0, 1.0, 0.5 };

// Sets the lighting for draw
void lighting() {
    glLightfv(GL_LIGHT0, GL_POSITION, position);
	glLightfv(GL_LIGHT0, GL_DIFFUSE, diffuse);
	//glLightfv(GL_LIGHT0, GL_AMBIENT, diffuse);
}

double s1x = 1.5;
double s1y = 3.0;
double s1z = 9;

double s2x = 3;
double s2y = 4;
double s2z = 11;

// Draws the graphics
void Draw() {
	glPushMatrix();

	glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT ); 

	// view transform
	cam.CreateLookAt();

	// lighting
	lighting();

	// world transforms
	glPushMatrix();


	// floor
	glBegin(GL_QUADS);
	glColor3f (1.0, 0.0, 0.0);
	glVertex3i(-8, 0, -10);
	glVertex3i(8, 0, -10);
	glVertex3i(8, 0, 8);
	glVertex3i(-8, 0, 8);
	glEnd();

	glPopMatrix();

	// spheres
	glPushMatrix();
	glColor3f (0.0, 1.0, 0.0);
	glTranslated(s1x, s1y, s1z);
	glutSolidSphere(1.0, 32, 32);
	glPopMatrix();

	glPushMatrix();
	glColor3f (0.0, 0.0, 1.0);
	glTranslated(s2x, s2y, s2z);
	glutSolidSphere(1.0, 32, 32);
	glPopMatrix();

	glPopMatrix();

	glFlush();

	glPopMatrix();
}

// Free up allocated memory
void Unload() {
}

// Handles window resizing
void reshape (int w, int h)
{
   glViewport (0, 0, (GLsizei) w, (GLsizei) h); 
   glMatrixMode (GL_PROJECTION);
   glLoadIdentity ();
   gluPerspective(54.0, (double) w / h, 0.01, 50.0);
   glMatrixMode (GL_MODELVIEW);
}

#define DELTA 0.1

// Handles keyboard input
void keyboard(unsigned char key, int x, int y) {
	putchar(key);
	
	switch(key) {
		case 'w':
			s1z -= DELTA;
			break;
		case 'a':
			s1x -= DELTA;
			break;
		case 's':
			s1z += DELTA;
			break;
		case 'd':
			s1x += DELTA;
			break;
		case 'r':
			s1y += DELTA;
			break;
		case 'f':
			s1y -= DELTA;
			break;

		case 'i':
			s2z -= DELTA;
			break;
		case 'j':
			s2x -= DELTA;
			break;
		case 'k':
			s2z += DELTA;
			break;
		case 'l':
			s2x += DELTA;
			break;
		case 'y':
			s2y += DELTA;
			break;
		case 'h':
			s2y -= DELTA;
			break;
	}

	glutPostRedisplay();
}

int _tmain(int argc, char** argv)
{
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_RGB | GLUT_DEPTH);
	glutInitWindowPosition(10, 10);
	glutInitWindowSize(RES_WIDTH,RES_HEIGHT);
	glutCreateWindow("Ray Tracer: Checkpoint 1");

	Initialize();
	
	glutDisplayFunc(Draw); 
	glutReshapeFunc(reshape);
	glutKeyboardFunc(keyboard);

	glutMainLoop();

	Unload();

	return 0;
}

