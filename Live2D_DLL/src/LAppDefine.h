/**
 *
 *  ���̃\�[�X��Live2D�֘A�A�v���̊J���p�r�Ɍ���
 *  ���R�ɉ��ς��Ă����p�����܂��B
 *
 *  (c) CYBERNOIDS Co.,Ltd. All rights reserved.
 */
#pragma once

//���
static const float VIEW_MAX_SCALE = 4.0f;
static const float VIEW_MIN_SCALE = 0.5f;

static const float VIEW_LOGICAL_LEFT = -1;
static const float VIEW_LOGICAL_RIGHT = 1;

static const float VIEW_LOGICAL_MAX_LEFT = -2;
static const float VIEW_LOGICAL_MAX_RIGHT = 2;
static const float VIEW_LOGICAL_MAX_BOTTOM = -2;
static const float VIEW_LOGICAL_MAX_TOP = 2;


// ���f����`-----------------------------------------------------------------------
static const int MODEL_HARU		=0;
static const int MODEL_HARU_A	=1;
static const int MODEL_HARU_B	=2;
static const int MODEL_SHIZUKU	=3;
static const int MODEL_WANKO    =4;

//�O����`�t�@�C��(json)�ƍ��킹��
static const char MOTION_GROUP_IDLE[]			="idle";//�A�C�h�����O
static const char MOTION_GROUP_TAP_BODY[]		="tap_body";//�̂��^�b�v�����Ƃ�

//�O����`�t�@�C��(json)�ƍ��킹��
static const char HIT_AREA_HEAD[]		="head";
static const char HIT_AREA_BODY[]		="body";

//���[�V�����̗D��x�萔
static const int PRIORITY_NONE  = 0;
static const int PRIORITY_IDLE  = 1;
static const int PRIORITY_NORMAL= 2;
static const int PRIORITY_FORCE = 3;

class LAppDefine {
public:
    static const bool DEBUG_LOG				= true;	//�f�o�b�O�p���O�̕\��
    static const bool DEBUG_TOUCH_LOG		= false;	//�^�b�`�����Ƃ��̏璷�ȃ��O�̕\��
	static const bool DEBUG_DRAW_HIT_AREA	= false;	//�����蔻��̉���

};

