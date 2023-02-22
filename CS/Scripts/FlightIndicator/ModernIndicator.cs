/// <summary>
/// Modern indicator. this script will display an F16 HUD
/// </summary>
using UnityEngine;
using System.Collections;

public class ModernIndicator : Indicator
{

	public Texture2D HlineTexture;
	public Texture2D HlineStepTexture;
	public Texture2D NumberShowArrowTexture;
	public Texture2D ArrowUpDownTexture;
	public Texture2D RulerHorionTexture;
	public Texture2D RelerVerticalTexture;

	private int GetShowVerticalAngle(float angle)
	{
		while (angle > 0)
			angle -= 360;
		angle %= 360;
		int resAngle = (int)angle;
		if (angle <= 0 && angle > -90)
			return resAngle;
		else if (angle <= -90 && angle > 180)
			return -180 - resAngle;
		else if (angle <= 180 && angle > 270)
			return resAngle + 270;
		else 
			return resAngle + 360;
	}

	private int GetHorionAngle(float angle)
    {
		int retAngle = (int)angle;
		while (retAngle < 0)
			retAngle += 360;
		retAngle %= 360;
		return retAngle;
    }

    protected override void OnGUI()
    {
        base.OnGUI();
		base.OnGUI();
		if (Show)
		{
			GUI.color = Color.green;
			switch (Mode)
			{
				case NavMode.Third:

					break;
				case NavMode.Cockpit:
					//if (Crosshair_in)
					//GUI.DrawTexture (new Rect ((Screen.width / 2 - Crosshair_in.width / 2) + CrosshairOffset.x, (Screen.height / 2 - Crosshair_in.height / 2) + CrosshairOffset.y, Crosshair_in.width, Crosshair_in.height), Crosshair_in);	
					DrawNavEnemy();

					Matrix4x4 matrixBackup = GUI.matrix;	//用于恢复正常UI绘制

					

					//旋转UI部分，根据机身旋转调整旋转（绕屏幕中心旋转）
					GUIUtility.RotateAroundPivot(this.gameObject.transform.rotation.eulerAngles.z, new Vector2(Screen.width / 2, Screen.height / 2));

					////绘制参考地平线
					if (HlineTexture)
					{
						//GUI.DrawTexture(new Rect((Screen.width / 2 - HlineTexture.width / 2), (Screen.height / 2 - HlineTexture.height / 2), HlineTexture.width, HlineTexture.height), HlineTexture);
						GUI.DrawTextureWithTexCoords(new Rect((Screen.width / 2 - HlineTexture.width / 2), -Screen.height/2, HlineTexture.width, Screen.height*2), 
							HlineTexture,new Rect(0,
							-transform.rotation.eulerAngles.x/180,
							1,1));
					}
					//Debug.Log(transform.rotation.eulerAngles.x);

					//绘制具体角度平线
					if (HlineStepTexture)
					{
						GUIStyle fontStyleLeft = new GUIStyle();
						fontStyleLeft.alignment = TextAnchor.MiddleRight;
						fontStyleLeft.normal.background = null;    //设置背景填充  
						fontStyleLeft.normal.textColor = new Color(0, 1, 0);   //设置字体颜色  
						fontStyleLeft.fontSize = 16;       //字体大小  

						GUIStyle fontStyleRight = new GUIStyle();
						fontStyleRight.alignment = TextAnchor.MiddleLeft;
						fontStyleRight.normal.background = null;    //设置背景填充  
						fontStyleRight.normal.textColor = new Color(0, 1, 0);   //设置字体颜色  
						fontStyleRight.fontSize = 16;       //字体大小  

						//绘制角度线
						GUI.DrawTextureWithTexCoords(new Rect((Screen.width / 2 - HlineStepTexture.width / 2), (Screen.height / 2 - 200), HlineStepTexture.width, 400), HlineStepTexture, new Rect(0, -this.gameObject.transform.rotation.eulerAngles.x/5, 1, 5));
						float angle = -transform.rotation.eulerAngles.x;
						float midLineOffset = ((int)angle % 5 + (angle - (int)angle)) * 15;
						//中间
						//绘制数字
						GUI.Label(new Rect(Screen.width / 2 - HlineStepTexture.width / 2-80, Screen.height / 2 - 15, 40, 30), (GetShowVerticalAngle(angle)).ToString(),fontStyleLeft);
						// 绘制指示箭头
						if(NumberShowArrowTexture)
							GUI.DrawTexture(new Rect(Screen.width / 2 - HlineStepTexture.width / 2 - 80, Screen.height / 2 - NumberShowArrowTexture.height / 2, NumberShowArrowTexture.width, NumberShowArrowTexture.height), NumberShowArrowTexture);
						int midLineAngle = (int)angle / 5 * 5;
						//左数字
						GUI.Label(new Rect(Screen.width / 2 - HlineStepTexture.width / 2-20 , Screen.height / 2 - 15 + midLineOffset, 40, 30), (GetShowVerticalAngle(midLineAngle) / 5 * 5).ToString(), fontStyleLeft);
						//右数字
						GUI.Label(new Rect(Screen.width / 2 + HlineStepTexture.width / 2 - 20, Screen.height / 2 - 15 + midLineOffset, 40, 30), (GetShowVerticalAngle(midLineAngle) / 5 * 5).ToString(), fontStyleRight);
						//Debug.Log(midLineAngle);
                        for (int i = 1; i <= 3; i++)
                        {
							//上侧数字
							if (i<3&&Screen.height / 2 + midLineOffset - 80 * i > Screen.height / 2 - 200)
							{
								//左
								GUI.Label(new Rect(Screen.width / 2 - HlineStepTexture.width / 2 - 20, Screen.height / 2 - 15 + midLineOffset - 80 * i, 40, 30), (GetShowVerticalAngle(midLineAngle + i * 5) / 5 * 5).ToString(), fontStyleLeft);
								//右
								GUI.Label(new Rect(Screen.width / 2 + HlineStepTexture.width / 2 - 20, Screen.height / 2 - 15 + midLineOffset - 80 * i, 40, 30), (GetShowVerticalAngle(midLineAngle + i * 5) / 5 * 5).ToString(), fontStyleRight);
							}
							
							//下侧数字
							if (Screen.height / 2 + midLineOffset + 80 * i < Screen.height / 2 + 200)
							{
								//左
								GUI.Label(new Rect(Screen.width / 2 - HlineStepTexture.width / 2 - 20, Screen.height / 2 - 15 + midLineOffset + 80 * i, 40, 30), (GetShowVerticalAngle(midLineAngle - i * 5) / 5 * 5).ToString(), fontStyleLeft);
								//右
								GUI.Label(new Rect(Screen.width / 2 + HlineStepTexture.width / 2 - 20, Screen.height / 2 - 15 + midLineOffset + 80 * i, 40, 30), (GetShowVerticalAngle(midLineAngle - i * 5) / 5 * 5).ToString(), fontStyleRight);
							}
						}
						//绘制侧身角度指向箭头
						if (Crosshair_in&&ArrowUpDownTexture&& (transform.rotation.eulerAngles.z <= 45 || transform.rotation.eulerAngles.z >= 315))
							GUI.DrawTexture(new Rect(Screen.width / 2 - ArrowUpDownTexture.width / 2, Screen.height / 2 + Crosshair_in.height / 2 + ArrowUpDownTexture.height / 2,
								ArrowUpDownTexture.width, ArrowUpDownTexture.height), ArrowUpDownTexture);

					}
					/*
					 * GUI.DrawTextureWithTexCoords函数
					 * 作用：根据具体图像的“缩放Rect”来改便图像在目标绘画的屏幕区域排布，绘制图像目标区域的部分将在相反的另一侧重新进入绘制区域
					 * 
					 * 第一参数Rect个参数是目标绘画的屏幕区域
					 * 
					 * 第二参数为绘制的目标图像（注意：参数目标贴图文件纹理类型应为“默认”否则会出现显示错误）
					 * 
					 * 第三参数的Rect参数是绘制目标绘画屏幕区域的“缩放Rect”（不是Rect的原意了）具体“缩放Rect”内的参数的意义如下：
					 * x：表示图x标方向的偏移图片个数量(值为负数时图片沿x轴镜像)
					 * y：表示图y标的偏移图片个数量(值为负数时图片沿y轴镜像)
					 * width：表示在目标绘制的屏幕区域内绘制几个重复填充的图像列
					 * height：表示在目标绘制的屏幕区域内绘制几个重复填充的图像行
					 * 
					 */


					//显示速度
					//GUI.matrix = matrixBackup;
					//GUI.skin.label.alignment = TextAnchor.UpperLeft;
					//GUI.Label(new Rect(Screen.width / 2 - 170, Screen.height / 2 - 150, 400, 30), flight.gameObject.GetComponent<Rigidbody>().velocity.magnitude.ToString());

					//不旋转部分UI，保持不变
					GUI.matrix = matrixBackup;  //恢复UI正常绘制
					//Debug.Log(transform.rotation.eulerAngles.y);
					//绘制方位角角度尺
					if(RulerHorionTexture)
                    {

						GUIStyle fontStyleMid = new GUIStyle();
						fontStyleMid.alignment = TextAnchor.MiddleCenter;
						fontStyleMid.normal.background = null;    //设置背景填充  
						fontStyleMid.normal.textColor = new Color(0, 1, 0);   //设置字体颜色  
						fontStyleMid.fontSize = 16;       //字体大小  

						GUI.DrawTextureWithTexCoords(new Rect(Screen.width / 2 -RulerHorionTexture.width*3/2 , Screen.height / 2 - 200 - 100, RulerHorionTexture.width*3, RulerHorionTexture.height),
							RulerHorionTexture, new Rect(transform.rotation.eulerAngles.y/10,0 , 3, 1),true);
						//绘制箭头
						if (ArrowUpDownTexture)
							GUI.DrawTexture(new Rect(Screen.width / 2 - ArrowUpDownTexture.width / 2, Screen.height / 2 - 200 - 100 + RulerHorionTexture.height+2, ArrowUpDownTexture.width, ArrowUpDownTexture.height), ArrowUpDownTexture);
						float angle = transform.rotation.eulerAngles.y;
						int midOffset = -(int)angle % 10 * 10;
						int MidAngle = (int)angle / 10 * 10;
						//Debug.Log(angle);
						//数字绘制
						//中间
						GUI.Label(new Rect(Screen.width / 2 - 20 + midOffset, Screen.height / 2 - 200 - 100 - 20, 40, 20), GetHorionAngle(MidAngle).ToString(), fontStyleMid);
                        for (int i = 1; i <= 4; i++)
                        {
							//左
							if(Screen.width / 2 - 20 + midOffset-i * 50>= Screen.width / 2 - RulerHorionTexture.width * 3 / 2)
								GUI.Label(new Rect(Screen.width / 2 - 20 + midOffset-i*50, Screen.height / 2 - 200 - 100 - 20, 40, 20), GetHorionAngle(MidAngle-i*10).ToString(), fontStyleMid);
							//右
							if (Screen.width / 2 - 20 + midOffset + i * 50 <= Screen.width / 2 + RulerHorionTexture.width * 3 / 2)
								GUI.Label(new Rect(Screen.width / 2 - 20 + midOffset + i * 50, Screen.height / 2 - 200 - 100 - 20, 40, 20), GetHorionAngle(MidAngle+i*10).ToString(), fontStyleMid);
						}
                    }

                    //绘制高度尺
                    if (RelerVerticalTexture&&NumberShowArrowTexture)
                    {
						GUIStyle fontStyleMid = new GUIStyle();
						fontStyleMid.alignment = TextAnchor.MiddleCenter;
						fontStyleMid.normal.background = null;    //设置背景填充  
						fontStyleMid.normal.textColor = new Color(0, 1, 0);   //设置字体颜色  
						fontStyleMid.fontSize = 16;       //字体大小  

						//绘制尺规
						GUI.DrawTextureWithTexCoords(new Rect(Screen.width / 2 + 200 + 20 + NumberShowArrowTexture.width, Screen.height / 2 - 200, RelerVerticalTexture.width, 400),
							RelerVerticalTexture, new Rect(-20, transform.position.y/100, 1, 5));
						//绘制中心显示箭头
						GUI.DrawTexture(new Rect(Screen.width / 2 + 200 + 20, Screen.height / 2 - NumberShowArrowTexture.height / 2, NumberShowArrowTexture.width, NumberShowArrowTexture.height),
							NumberShowArrowTexture);

						int height = (int)transform.position.y;
						//显示箭头数字
						GUI.Label(new Rect(Screen.width / 2 + 200 + 20, Screen.height / 2 - NumberShowArrowTexture.height / 2, NumberShowArrowTexture.width,NumberShowArrowTexture.height), 
							height.ToString(), fontStyleMid);
						int midOffset = (int)((float)(height % 100/1.3f));
						//Debug.Log(midOffset);
						//数字绘制

						int rulerShow = height / 100 * 100;
						//中间
						GUI.Label(new Rect(Screen.width / 2 + 200 + 20 + NumberShowArrowTexture.width + RelerVerticalTexture.width, Screen.height / 2 - 10+midOffset,
							NumberShowArrowTexture.width, 20), rulerShow.ToString(), fontStyleMid);
						int offset = 40;
                        for (int i = 1; i <= 5; i++)
                        {
							//上侧
							if(Screen.height/2-i* offset >= Screen.height / 2 - 200-offset )
								GUI.Label(new Rect(Screen.width / 2 + 200 + 20 + NumberShowArrowTexture.width + RelerVerticalTexture.width, Screen.height / 2 - 10 + midOffset-i* offset,
							NumberShowArrowTexture.width, 20), (rulerShow + 100*i).ToString(), fontStyleMid);
							//下侧
							if(Screen.height/2+i* offset <= Screen.height / 2 + 200-2*offset)
								GUI.Label(new Rect(Screen.width / 2 + 200 + 20 + NumberShowArrowTexture.width + RelerVerticalTexture.width, Screen.height / 2 - 10 + midOffset+i* offset,
							NumberShowArrowTexture.width, 20), (rulerShow - 100*i).ToString(), fontStyleMid);
						}
                    }
					

					break;
				case NavMode.None:

					break;
			}
		}
	}

}
