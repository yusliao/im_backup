   M S B A M L   `   `   `  ���� C  ?IM.Emoje, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG1clr-namespace:IM.Emoje.Controls;assembly=IM.EmojeIM.Emoje.Controls  C ?CSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:CSClient.Views.ControlsCSClient.Views.Controls 1   +CSClient.Views.ChildWindows.ImageScanWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     DgifImg1clr-namespace:IM.Emoje.Controls;assembly=IM.Emoje     7local%clr-namespace:CSClient.Views.Controls     $��10q�  � 
ResizeMode$  CanMinimize=�$��微软雅黑)� � WindowStyle$
 None=� � WindowStartupLocation$ CenterScreen=� � AllowsTransparency .  � Title$ 图片预览��$	��600��$	��800����Z�  J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     Opacity$
 0.25Q�   
BlurRadius$ 10Q�   Color$ #000000��	   	Direction$	 90Q�
   ShadowDepth$
 0Q�.��� -    �Name$ 
gridLayout��
������� � RowDefinitions �� $��26 ��� �� $��40 �.+���  �� Orientation$ 
Horizontal=�$��Right=�.+��� -    ��Name$ btnMin��   MinButtonStyle#��[  
 J� Uid$	 Min��d� -    d�Name$ btnMax��  MaxButtonStyle#��[ $	 Max���� -   $ btnClose��  CloseButtonStyle#��[ $ Close���� $��1��}�. ._�� -   $ gridView��
������ .+�� -   $ gridScan�� J� RenderTransformOrigin$ 0.5,0.5&�z�V� .~��� -   	scale��T� -   	translate���� -	   
rotate��.+�    IM.Emoje.Controls.GifImage  -
     Name$	 img��$��Center=�$��Center=�n� -    n�Name$ thumb�� J� IsHitTestVisible .  ����  �� 
TargetType" n�.R��� 
������ �� $ 
Horizontal=�$��2��$��Center=�.+���   RectPathButtonStyle#��[  � ToolTip$ 放大��$ Expand��  	GeoExpand
 � Tag# [ �� #��[ $ 缩小��$ Shrink��  	GeoShrink# [ �� #��[ $ 
旋转90°��$ Rotate��  	GeoRotate# [ �� #��[ $ 保存图片��$
 Save��  GeoDownload# [ 