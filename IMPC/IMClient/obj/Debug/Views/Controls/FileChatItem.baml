   M S B A M L   `   `   `  ����?  ;IMUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:IMClient.ConverterIMClient.Converter  5      *    $IMClient.Views.Controls.FileChatItem   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      /cv clr-namespace:IMClient.Converter     5      $��微软雅黑)�6:      DarkForeground#��[  5        CommonFontSize#��[ 6<     � 	Resources  5      *   $IMClient.Converter.FileNameConverter  fileNameToView*   $IMClient.Converter.FileSizeConverter  fileSizeToView.   (IMClient.Converter.BoolConvertVisibility  btv��%W   &	       &	      &	 &       5   
   6     5   
   6     5	   
   6#   5
      .��5      �� -   5         ��Name$ bdLayout��5       �� CornerRadius$ 3��6   $��1q�60     CommonBorderBrush#��[ 6D   
�������._�5V   
   � -   5         �Name$ 
gridLayout��5V      $	��229��6X   $��77��6d   $��	12,5,12,0q�6q   .+�5W      � -   5        $ gridFile��5W      
������� � ContextMenu 5X      �� 5Y      -   5         ��Name$
 menu��5Y   "   5b       � ColumnDefinitions 5d      �� 5e      $
��Auto �6'   �� 5f      .+�5h      �� -   5         ��Name$	 img��5h      $��3��6&   $��0,0,10,0q�6l   $��55��6~   $��55��6�   $
��Fill=�6�   ��67   �� *ResourceModel.FileImg+ �� Mode$ OneWay=�6�   � 5i      -   5        $ 
gridUplaod��5i      $��Bottom=�6+   $��Right=�6F   $��	0,0,10,12q�6b   $y�	Collapsed=�6u   .+�5j      A� $��11��6   $��11��6)   ���#000000	 J� Opacity$		 0.5Q�6D   F� 5k      
�������$��0,0,4,1q�6)   ������n� �n� ��n� �n� S�n� gfffff#@gfffff"@�� �͇ �� �433333@�� �������@�� 433333@gfffff#@433333@�n� �433333@�n� �433333@�ڊ ������@�͇ 433333@�͇ Ѡ͇ �͇ Ӏa� �͇ �n� �ڊ �n� �n� 	6:   F� 5l      
�������$��0,0,1,5q�6(   �����͇ ��M gfffff!@������@�,� ������@gfffff @��M � Ii `!` � Ii �2) S Ii 433333@�;f ��!  �a ��! �gfffff@��! �Z 433333@�Z �2) ��Z  �a �`�G ��M � �D ��J @@ ��J  	= ��M ���9  �P ��9  sU  	= @�X � ] `�x �`!` ��{ @�d ��{ ��g `�x Q�͇ 433333@��ڊ  sU �ڊ  �P �͇ ��M 	69   5m      � 5n      -   5        $ gridUplaodCompleted��5n      $��Bottom=�64   $��Right=�6O   $��	0,0,10,12q�6k   $y�	Collapsed=�6~   .+�5o      A� $��11��6   $��11��6)   ���#000000$		 0.5Q�6D   F� 5p      
�������$��0,0,3,4q�6)   ����� �  �D ��� ��A  |� ��A �n�  �D �@�X ��{ �ffffff@�.c �233333@`!` ������@`!` 233333@�.c � �0 @�d  �0 ��j @�3  �m ��eR �a� gfffff@������"@�Z ������"@gfffff@�a� Q��� 233333@�`*� @KL `*� `�G  �  �D 	6;   5r      � 5t      $��1��6   $
��0,11q�6'   
 � RowDefinitions
 5u      �� 5v      �� 5w      �� 5x      $
��Auto �6(   .+�5{      �� -   5         ��Name$ tbName��5{       
���   �
������ $	��Top=�6\   $��微软雅黑)�6t   $��1��6�   $��0,0,12,0q�6�     LowerFontSize#��[ 6  ��6�   �� *ResourceModel.FileName+ �� 	Converter# [ $ OneWay=� �� ConverterParameter$ 10��6  �� 5|      -	   5        $ tbSize��5|       $��1��60   $��1��6@   $��0,0,12,0q�6M   $��Center=�6�     LightForeground#��[ 6�   #��[ 6  ��6_   �� *ResourceModel.Length+# [ $ OneWay=�6  � 5}      -
   5         �Name$ 	pbProcess��5}   "   $��1��65   $��2��6E   $��4��6R   ���#4EC520$��0q�6s   $��0,3,12,3q�6�    � Maximum 5~      �� *ResourceModel.Length+$ OneWay=� � Value 6O   �� *ResourceModel.CompleteLength+$ OneWay=�6�   5      5�      � 5�      -   5        $ gridScan��5�      $y�	Collapsed=�6%   #��[ 6<   $��10��6l   $��10��6~   }�. .+�5�      �� $��Bottom=�6    �� Padding$ 5,30,0,5q�67   $��
0,0,-100,0q�6J    J� IsHitTestVisible .  
�������#��[ 5�   )   ��5�      �� 5�       �� 
StartPoint$ 0.5,0&�6.    �� EndPoint$ 0.5,1&�6A   .��5�      �  � Offset$ 0Q�6+    � Color$ Transparent��66   � 5�      $ 1Q�6+   $ 	#AA333333��66   5�      �� 5�      �6   �� *ResourceModel.FileName+# [ $ OneWay=�$ 6���� 5�      �� 5�      �6   �� *ResourceModel.Length+# [ $ OneWay=�5�      5�      5�      5�      5�      