   M S B A M L   `   `   `  ���� C  ?CSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:CSClient.ConverterCSClient.Converter  *    $CSClient.Views.Controls.FileChatItem   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     /cv clr-namespace:CSClient.Converter     $��微软雅黑)�   DarkForeground#��[    CommonFontSize#��[   � 	Resources  *   $CSClient.Converter.FileNameConverter  fileNameToView*   $CSClient.Converter.FileSizeConverter  fileSizeToView.   (CSClient.Converter.BoolConvertVisibility  btv��%-   &	       &	      &	 
           .���� $	��220��$��80�� �� CornerRadius$ 3��$��1q�  CommonBorderBrush#��[ 
�������._�� -    �Name$ 
gridLayout��
������ $��	12,5,12,0q� � ContextMenu �� -    ��Name$
 menu�� � RowDefinitions �� �� �� $
��Auto ���  � ColumnDefinitions �� $
��Auto ��� �� $��45 �.+��� -    ��Name$	 img��$��2��$��3��$��45��$��45��$
��Fill=����� *ResourceModel.FileImg+ �� Mode$ OneWay=��� -   	 ��Name$	 BdIcon��$y�	Collapsed=�$��1��$��2��$��Right=�$��2q�$��15��$��15��$ 10��
 J� IsHitTestVisible
 .  
��� � �._�F� -    F�Name$ pathIcon��  GeoHook#��[ 
������� �� Stretch$ UniformToFill=�$��3q�$��2���� -    ��Name$ tbName��
���   �
������ $��微软雅黑)�$��1��$��0,0,12,0q�$��Center=�  LowerFontSize#��[ ���� *ResourceModel.FileName+ �� 	Converter# [ $ OneWay=� �� ConverterParameter$ 10���� -   $ tbSize��$��1��$��1��$��0,0,12,0q�$��Center=�  LightForeground#��[ #��[ ���� *ResourceModel.Length+# [ $ OneWay=�� -    �Name$ 	pbProcess��$��1��$��2��$��4�����#4EC520$��0q�$��0,3,12,3q� � Maximum �� *ResourceModel.Length+$ OneWay=� � Value �� *ResourceModel.CompleteLength+$ OneWay=��� $��2��$��3�� 	 LineRectStyle#��[	 #��[ $	��0.5��A� -	    A�Name$ ugState��$��3��$��3�� A� Rows$ 1��$��0,0,10,0q�{��� *
CanOperate+$ OneWay=�   �� 
TargetType 
 ButtonItemStyle �� BasedOn �� Property �� Value��%<   '��       7 
 �� " ��8   .���  � �� �#4C68B9.+��� -
   $ tbConnecting�����#999999$��传输连接中��$y�	Collapsed=��� -    ��Name$ btnOfflineSend��
 J� Uid$ OfflineSend��$��转离线发送��$
��Left=��� -   $ btnOnlineSend��$ 
OnlineSend��$��转在线发送��$
��Left=��� -   $ 
btnReceive��$ Receive��$��接收��$
��Left=��� -   $ btnOpen��$
 Open��$��打开��$
��Left=��� -   $ 	btnSaveAs��$ SaveAs��$��	另存为��$��Center=��� -   $ 	btnDelete��$ Delete��$��删除��$��Right=��� -   $ 	btnCancel��$ Cancel��$��取消��$��Right=�