/**
 *
 *  ���̃\�[�X��Live2D�֘A�A�v���̊J���p�r�Ɍ���
 *  ���R�ɉ��ς��Ă����p�����܂��B
 *
 *  (c) CYBERNOIDS Co.,Ltd. All rights reserved.
 */

// Live2D
#include <stdlib.h>

#include	"Live2D.h"
#include	"util/UtDebug.h"



/************************************************************
* Live2D�p�������m�ۖ��߂̃J�X�^�}�C�Y
* live2d::LDAllocator�̉��z�֐����I�[�o�[���C�h����
************************************************************/
class MyLive2DAllocator : public live2d::LDAllocator{
public:

	
	MyLive2DAllocator() {
	}

	virtual ~MyLive2DAllocator() {
	}

	//---------------------------------------------------------------------------
	//	����������
	//
	//  Live2D::init()����Ă΂�鏉���������B�K�v�ɉ����ď������������s��
	//---------------------------------------------------------------------------
	virtual void init(){ }

	//---------------------------------------------------------------------------
	//�@�I������
	//
	//�@Live2D::dispose()�̍Ō�ɌĂ΂��I�������B�K�v�ɉ����ďI���������s��
	//---------------------------------------------------------------------------
	virtual void dispose(){ }


	//---------------------------------------------------------------------------
	// malloc
	//
	// �����������������Ă�����x�̃T�C�Y�i1024�ȏ�j�ŌĂ΂��
	//---------------------------------------------------------------------------
	virtual void* pageAlloc( unsigned int size , LDAllocator::Type  allocType ){
		void* ptr ;
		switch( allocType ){
		case LDAllocator::MAIN:	//�ʏ�̃�����
			ptr = ::malloc(size) ;
			break ;

		case LDAllocator::GPU:	//GPU�ɓn�����_�A�C���f�b�N�X�AUV
			ptr = ::malloc(size) ;
			break ;

		default:				//���̑��i����ǉ�����\������j
			L2D_DEBUG_MESSAGE( "Alloc type not implemented %d" , allocType ) ;
			ptr = ::malloc(size) ;
			break ;
		}

		L2D_ASSERT_S( ptr != NULL , "MyAllocator#malloc failed (size= %d)" , size ) ;
		return ptr ;
	}

	//---------------------------------------------------------------------------
	// free
	//---------------------------------------------------------------------------
	virtual void pageFree( void* ptr , LDAllocator::Type  allocType ){
		::free(ptr);
	}

} ;

