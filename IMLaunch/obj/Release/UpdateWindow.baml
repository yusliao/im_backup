   M S B A M L   `   `   `  ���� C  ?IMLaunch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:IMLaunchIMLaunch      IMLaunch.UpdateWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:IMLaunch     -     �Name$
  root�� � AllowsTransparency .  � WindowStyle$
 None=� � 
ResizeMode$ CanMinimize=� � ShowInTaskbar .  � WindowStartupLocation$ CenterScreen=� � Topmost .    ShadowWindowStyle#���    � Title$ 更新��$	��340��$	��420��{��� *IsEnabledWindow+ � 	Resources    BooleanToVisibilityConverter2   ,IMLaunch.ReverseBooleanToVisibilityConverter ' #ReverseBooleanToVisibilityConverter��%   &	       &	      ��   .���� ���#F8F8F8._�� -   
������ .+��� $��Right=�$	��Top=�  
CloseStyle#���  	 K� ZIndex$
	 9999��$��0q����� *
CmdClose+�� -   
 ��Name$
 btnMin��$��Right=�$	��Top=�  MinimizeStyle#���  $
	 9999��$��0,0,32,0q���  �� Orientation$ Vertical=�.+��� $,��&/IMLaunch;component/Image/V2/Logo2.png��$	��121��$��93��$��0,50,0,0q��� -    ��Name$ 	tbVersion��$��14(����#4E546A$��Center=�$��0,12,0,0q����� *	Version+�� -    ��Name$ progressPnl��$ Vertical=�$��0,12,0,0q�y��� *	Installed+ �� 	Converter# [ .+�� $	��300��$��3�� � Minimum$ 0Q� � Maximum$	 100Q� � Value �� *
Progress+����  �� 
TargetType" �.���  �� Property � �� �� Value �#EEF4FF��  � �� .  ��  � �� �#4C68B9��  �@J�SnapsToDevicePixels . ��  � �� ��  �� 
TargetType" �.R��  �Name$
 Root��.+���  ��Name$ 
PART_Track��#��z�� J� SnapsToDevicePixels# z  �� CornerRadius$ 0���� $ PART_Indicator��$
��Left=�#��z��# z $ 0�� �� Triggers P�  P� Property �@�Orientation P� Value$ Vertical=�. ���  �� 
TargetName$
 Root�� �@�LayoutTransform ��  �� Angle$	 -90Q��� $��15(����#4E546A$��Center=�$��0,20,0,0q����� *
Progress+ �� StringFormat$ 已更新 {0}%���� $��15(����#4E546A$��Center=�$��0,20,0,0q����� *Description+  � ToolTip  �� *Description+�� $��继续使用旧版本��$��0,10,0,0q�$	��130��$��30��  TextButtonStyle2#���  ���� *CmdStartExe+y��� *UserOldVersion+# [ �� $ Vertical=�$��0,20,0,0q�y��� *	Installed+# [ .+��� $��	已安装��$��16(����#C3C9D5$��Center=��� $��开始使用��$��0,30,0,0q�$	��130��$��30��#���  ���� *CmdStartExe+