/**
 *
 *  ���̃\�[�X��Live2D�֘A�A�v���̊J���p�r�Ɍ���
 *  ���R�ɉ��ς��Ă����p�����܂��B
 *
 *  (c) CYBERNOIDS Co.,Ltd. All rights reserved.
 */

#include "FileManager.h"
#include "MyLive2DAllocator.h"
#include <stdio.h>
#include "type/LDString.h"


char* FileManager::loadFile(const char* filepath,int* ret_bufsize)
{
		FILE *fp; // (1)�t�@�C���|�C���^�̐錾
		char * buf;
	
		// (2)�t�@�C���̃I�[�v���B�����ŁA�t�@�C���|�C���^���擾����
		if ( fopen_s( &fp , filepath, "rb") ) //return nonzero if error
		{
			L2D_DEBUG_MESSAGE("file open error %s!!" , filepath );
			return NULL;
		}
	
		//------------ �T�C�Y�̔��� ------------
		fseek(fp, 0, SEEK_END );
		int size = ftell(fp);//�T�C�Y���擾
	
		buf = (char*)malloc( size );//�������̊m�ہi�O���ŊJ���j
		L2D_ASSERT_S( buf != 0 , "malloc( %d ) is NULL @ fileload %s" , size , filepath ) ;
		
	
		fseek(fp, 0, SEEK_SET);
	
		// �ǂݍ���
		int loaded = (int)fread(buf, sizeof(char), size, fp);
		fclose(fp); // (5)�t�@�C���̃N���[�Y
	
		//------------ ���������ǂݍ��߂������� ------------
		if (loaded != size)
		{
			L2D_DEBUG_MESSAGE("file load error / loaded size is wrong / %d != %d\n" , loaded , size );
	
	
			return NULL;
		}
	
		*ret_bufsize = size ;
		return buf;

}


void FileManager::releaseBuffer(void* ptr)
{
	free(ptr);
}


/************************************************************
@title	�e�N�X�`���ǂݍ���
@subt
************************************************************/
void FileManager::loadTexture( LPDIRECT3DDEVICE9 g_pD3DDevice, const char * textureFilePath, LPDIRECT3DTEXTURE9* tex)
{
	if( FAILED( D3DXCreateTextureFromFileExA( g_pD3DDevice
				, textureFilePath 
				, 0	//width 
				, 0	//height
				, 0	//mipmap( 0�Ȃ犮�S�ȃ~�b�v�}�b�v�`�F�[���j
				, 0	//Usage
				, D3DFMT_A8R8G8B8                
				, D3DPOOL_MANAGED
				, D3DX_FILTER_LINEAR
				, D3DX_FILTER_BOX
				, 0			  
				, NULL
				, NULL
				, tex ) ) )
	{
		L2D_DEBUG_MESSAGE("Could not create texture \n" , textureFilePath );
		return ;
	}
}


/************************************************************
@title	�e�f�B���N�g���̎擾
@subt
************************************************************/
void FileManager::getParentDir( const char* path , live2d::LDString* return_dir ){
	(*return_dir) = path ;
	(*return_dir) += "\\..\\" ;

}
