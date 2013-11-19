/**
 *
 *  ���̃\�[�X��Live2D�֘A�A�v���̊J���p�r�Ɍ���
 *  ���R�ɉ��ς��Ă����p�����܂��B
 *
 *  (c) CYBERNOIDS Co.,Ltd. All rights reserved.
 */
#include "LAppLive2DManager.h"

#include "L2DViewMatrix.h"

//Live2DApplication
#include "LAppModel.h"
#include "LAppDefine.h"
#include "LAppModel.h"
#include "L2DMotionManager.h"


using namespace live2d;

/**
 * �R���X�g���N�^
 */
LAppLive2DManager::LAppLive2DManager()
	:modelIndex(0)
{
	//  �ȉ��̖��߂Ń��������[�N�̌��������{�i_DEBUG���[�h�̂݁j
	//�@Live2D::dispose()���ɁALive2D�̊Ǘ����郁�����Ń��[�N���������Ă����ꍇ�Ƀ_���v����
	//	���[�N���Ă���ꍇ�́AMEMORY_DEBUG_MEMORY_INFO_ALL�ł��ڍׂȏ����_���v���܂�
	//�@�����p�̃f�[�^��global new���Z�q���g���܂� 
//	live2d::UtDebug::addMemoryDebugFlags( live2d::UtDebug::MEMORY_DEBUG_MEMORY_INFO_COUNT ) ;//���������[�N�̌��o�p

	// Live2D������
	live2d::Live2D::init( &myAllocator );
}


/**
 * �f�X�g���N�^
 */
LAppLive2DManager::~LAppLive2DManager() 
{
	releaseModel();
	Live2D::dispose();
}


void LAppLive2DManager::releaseModel()
{
	for (unsigned int i=0; i<models.size(); i++)
	{
		delete models[i];
	}
    models.clear();
}


void LAppLive2DManager::setDrag(float x, float y)
{
	for (unsigned int i=0; i<models.size(); i++)
	{
		models[i]->setDrag(x, y);
	}
}


/**
 * �^�b�v�����Ƃ��̃C�x���g
 * @param tapCount
 * @return
 */
bool LAppLive2DManager::tapEvent(float x,float y)
{
	if(LAppDefine::DEBUG_LOG) UtDebug::print( "tapEvent\n");
	
	for (unsigned int i=0; i<models.size(); i++)
	{
		if(models[i]->hitTest(  HIT_AREA_HEAD,x, y ))
		{
			//����^�b�v������\��؂�ւ�
			if(LAppDefine::DEBUG_LOG)UtDebug::print( "face\n");
			models[i]->setRandomExpression();
		}
		else if(models[i]->hitTest( HIT_AREA_BODY,x, y))
		{
			if(LAppDefine::DEBUG_LOG)UtDebug::print( "body\n");
			models[i]->startRandomMotion(MOTION_GROUP_TAP_BODY, PRIORITY_NORMAL );
		}
	}
	    
    return true;
}


//���f����ǉ�����
void LAppLive2DManager::changeModel()
{
	if(LAppDefine::DEBUG_LOG)UtDebug::print("model index : %d\n",modelIndex );	
	switch (modelIndex)
	{
	case 0://�n��
		releaseModel();
		models.push_back(new LAppModel());
		models[0]->load( MODEL_HARU ) ;
		break;
	case 1://������
		releaseModel();
		models.push_back(new LAppModel());
		models[0]->load( MODEL_SHIZUKU ) ;
		break;
	case 2://���
		releaseModel();
		models.push_back(new LAppModel());
		models[0]->load( MODEL_WANKO ) ;
		break;
	case 3://�������f��
		releaseModel();
		models.push_back(new LAppModel());
		models[0]->load( MODEL_HARU_A ) ;
				
		models.push_back(new LAppModel());
		models[1]->load( MODEL_HARU_B ) ;
		break;
	default:		
		break;
	}
	modelIndex++;
	modelIndex = modelIndex%4;
}

void LAppLive2DManager::deviceLost(){
	if(LAppDefine::DEBUG_LOG) live2d::UtDebug::print( "DeviceLost @LAppLive2DManager::deviceLost()\n");

	for (unsigned int i=0; i<models.size(); i++)
	{
		models[i]->deviceLost() ;
	}
}
