   M S B A M L   `   `   `  ����C  ?IM.Emoje, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG1clr-namespace:IM.Emoje.Controls;assembly=IM.EmojeIM.Emoje.Controls  5   	   C ?CSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:CSClient.Views.ControlsCSClient.Views.Controls 5   	   1   +CSClient.Views.ChildWindows.ImageScanWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   DgifImg1clr-namespace:IM.Emoje.Controls;assembly=IM.Emoje     5   	   7local%clr-namespace:CSClient.Views.Controls     5   	   $��10q�5        � 
ResizeMode$  CanMinimize=�6/   $��微软雅黑)�5   =    � WindowStyle$
 None=�5	   	    � WindowStartupLocation$ CenterScreen=�6    � AllowsTransparency .  � Title$ 图片预览��5
   	   $	��600��6   $	��800��6#   ��6/   Z�  J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5   
      Opacity$
 0.25Q�6      
BlurRadius$ 10Q�6*      Color$ #000000��6:   	   	Direction$	 90Q�6J   
   ShadowDepth$
 0Q�6Y   .��5      � -   5         �Name$ 
gridLayout��5      
������� � RowDefinitions 5   
   �� 5      $��26 �6   �� 5      �� 5      $��40 �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��Right=�6.   .+�5      �� -   5         ��Name$ btnMin��5         MinButtonStyle#��[  6%   
 J� Uid$	 Min��6M   d� 5      -   5         d�Name$ btnMax��5        MaxButtonStyle#��[ 6+   $	 Max��6S   �� 5      -   5        $ btnClose��5        CloseButtonStyle#��[ 6'   $ Close��6Q   5      �� 5   
   $��1��6   }�. ._�5      � -   5        $ gridView��5      
������ .+�5       � -   5        $ gridScan��5        J� RenderTransformOrigin$ 0.5,0.5&�6)   z�5!      V� 5"      .~�5#      �� -   5        	scale��5#   -   T� 5$      -   5        	translate��5$   1   �� 5%      -	   5        
rotate��5%   .   5&      .+�5)          IM.Emoje.Controls.GifImage  -
   5          Name$	 img��5)   &   $��Center=�63   $��Center=�6P   n� 5,      -   5         n�Name$ thumb��5,       J� IsHitTestVisible .  ��5-      �� 5.       �� 
TargetType" n�6.   .R�5/   "   �� 
������ 6)   50      52      53      54      55      �� 57   
   $ 
Horizontal=�6   $��2��6.   $��Center=�6;   .+�58      ��   RectPathButtonStyle#��[ 6    � ToolTip$ 放大��6H   $ Expand��6U     	GeoExpand
 � Tag# [ 6b   �� 59      #��[ 6   $ 缩小��6H   $ Shrink��6U     	GeoShrink# [ 6b   �� 5:      #��[ 6   $ 
旋转90°��6B   $ Rotate��6R     	GeoRotate# [ 6_   �� 5;      #��[ 6   $ 保存图片��6B   $
 Save��6Q     GeoDownload# [ 6\   5<      5=      5>      