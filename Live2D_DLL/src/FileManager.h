/**
 *
 *  ���̃\�[�X��Live2D�֘A�A�v���̊J���p�r�Ɍ���
 *  ���R�ɉ��ς��Ă����p�����܂��B
 *
 *  (c) CYBERNOIDS Co.,Ltd. All rights reserved.
 */

#include <d3dx9.h>
#include "type/LDString.h"


class FileManager 
{
public:
	static char* loadFile(const char* path,int* size);
	static void releaseBuffer(void* ptr);
	static void loadTexture( LPDIRECT3DDEVICE9 g_pD3DDevice, const char * textureFilePath, LPDIRECT3DTEXTURE9* tex) ;

	//�e�̃f�B���N�g���� return_dir �ɃZ�b�g����
	static void getParentDir( const char* path , live2d::LDString* return_dir ) ;
};

