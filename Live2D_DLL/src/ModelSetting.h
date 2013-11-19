/**
 *
 *  ���̃\�[�X��Live2D�֘A�A�v���̊J���p�r�Ɍ���
 *  ���R�ɉ��ς��Ă����p�����܂��B
 *
 *  (c) CYBERNOIDS Co.,Ltd. All rights reserved.
 */
#pragma once



class ModelSetting
{
public:
    
	virtual ~ModelSetting(){}
	
	//���f���f�[�^�ɂ���
	virtual const char* getModelName() =0 ;
	virtual const char* getModelFile() =0  ;
	
	//�e�N�X�`���ɂ���
	virtual int getTextureNum() =0 ;
	virtual const char* getTextureFile(int n) =0 ;
	
	//�����p�����[�^�ɂ���
	virtual int getInitParamNum() =0;
	virtual float getInitParamValue(int n) =0 ;
	virtual const char* getInitParamID(int n) =0 ;
	
	//�����p�[�c�\���ɂ���
	virtual int getInitPartsVisibleNum() =0 ;
	virtual float getInitPartsVisibleValue(int n) =0 ;
	virtual const char* getInitPartsVisibleID(int n) =0;
	
	//�����蔻��ɂ���
	virtual int getHitAreasNum() =0 ;
	virtual const char* getHitAreaID(int n) =0 ;
	virtual const char* getHitAreaName(int n) =0 ;
	
	//�������Z�A�p�[�c�؂�ւ��A�\��t�@�C���ɂ���
	virtual const char* getPhysicsFile() =0;
	virtual const char* getPoseFile() =0;
	
	virtual int getExpressionNum()=0;
	virtual const char* getExpressionName(int n) =0 ;
	virtual const char* getExpressionFile(int n) =0 ;
	
	//���[�V�����ɂ���
	virtual int getMotionNum(const char* name)  =0;
	virtual const char* getMotionFile(const char* name,int n) =0   ;
	virtual const char* getMotionSound(const char* name,int n)  =0 ;
	virtual int getMotionFadeIn(const char* name,int n) =0 ;
	virtual int getMotionFadeOut(const char* name,int n) =0     ;
	
	virtual bool getLayout(live2d::LDMap<live2d::LDString, float> & layout)=0;
	
};