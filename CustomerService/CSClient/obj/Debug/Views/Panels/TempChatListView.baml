   M S B A M L   `   `   `  ����C  ?CSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:CSClient.ConverterCSClient.Converter  5      ,    &CSClient.Views.Panels.TempChatListView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      /cv clr-namespace:CSClient.Converter     5        � 	Resources  5	          BooleanToVisibilityConverter2  ,CSClient.Converter.DataTimeToStringConverter  dts4   .CSClient.Converter.UnReadMessageCountConverter  	unreadMsg'  !CSClient.Converter.HeadImgConvter  htc �� 
TargetType �� Property �� Value �� 
TargetType ��Name F�Name �Name � ContextMenu	 m� Command
 � RowDefinitions � ColumnDefinitions �� ImageSource �� Mode �� 	Converter ��Name  CommonFontSize � ToolTip �� RelativeSource  LowerFontSize ��Name  LightForeground ��Name ��Name  AppendLabelStyle �� Triggers �� Value �� Binding �� 
TargetName ^� 
Conditions �� Binding �� Value P� Property P� Value  MouseOverBackground 	 SelectedBackground��%�  &	        &	      &	 &     &	 9     '�� L     7  7  7  7  7  7  7  7  7  7  7  7 	 7  �� 5
   
   6'     5   
   6'     5   
   6)     5   
   6   �� 5U   
   " ��6   .�5V      ��  �@�FocusVisualStyle 61   Z� �� 5W       � ��$ 0q�60   �� 5X       � �� 6,   Z� �� 5Y       � ��
 ���� 6+   �� 5Z       � �� 5[      �� 5\      " ��6&   .R�5]      �� $ bd��6!   #��z��6-   $��0,0.5q�6W   ._�5^      � .+�5_   "   F� $ pathTopMost��6'   $y�	Collapsed=�6<   "����        �    ��� р��     	$��5q�6m   $
��Left=�6x   $	��Top=�6�   ���#688FFF6�   � 5`   "   $
 grid��6'   -   5        $��50��5`   5   $
��5,10q�6A   $	��220��6O   
������  5a   &   �� 5b   *   .@�5g   .   m� $��领取任务��67   	 6E   �� *PreemptChatCommand+5h   +   
 5j   &   �� 5k   *   �� 5l   *    5n   &   �� 5o   *   $��50 �6;   �� 5p   *   �� 5q   *   $
��Auto �6;   .+�5t   &   A� $��2��6.   $��6q�6?   ��5u   *   �� 5v   .    69   �� *Chat.HeadImg+$ OneWay=�8   5x   '   �� 5y   &   $ tbDisplayName��60   $��1��6u   $	��130��6�   $��CharacterEllipsis=�6�   ���#333333$
��Left=�5{   2   $��Center=�6N   8�� ��5y   G   �� *Chat.DisplayName+$ OneWay=� 5z   G   �� *Text+ �� *Self+$ OneWay=�5{   i   �� 5|   &   $��2��5}   -   $��Center=�6=   ���#9999998�� ��5|   0   �� *Model.LastMsg.SendTime+8  $ OneWay=�5}   m   ]� 5   &   $��1��60   $��1��6@   $��2��6M   ��. $��Center=�6v   .+�5�   *   �� $ imgNodisturb��60   $y�	Collapsed=�6F   $3��-/IMAssets;component/Images/icon-nodisturb.png��5�   -   $��Right=�6d   $��12��5�   -   $��12��68   $��10,0,0,0q�6D   �� 5�   *   $ tbUnReadCount��64   $
��Left=�6K   8�� 8�� �� 5�   .   $ runSenderLeft��62   $�[��6I    �� 5�   .   $ runSenderMiddle��62   �6K   �� *UnReadCount+8  $ OneWay=� �� 5�   .   $ runSenderRight��62   $
�条]��6J   5�   +   �� 5�   *   $��CharacterEllipsis=�64   8�� ���#999999�� 5�   /   $ 
runSender0��63    �� 5�   /   $ 
runSender1��63    �� 5�   /   �63   �� *LastMsg.Content+5�   +   5�   '   �� 5�   &   $ lblUnReadCount��6,   8�� $��Right=�6n   $	��Top=�6�   
������ $��	0,10,-5,0q�5�   :   $��2��6M   ��6^   �� *Model.Chat.IsNotDisturb+��5�       �� *UnReadCount+8 	 $ OneWay=�5�   #   5�      5�       5�      �� 5�      $
 true��6R    6*   �� *	IsGroup+$ OneWay=�.a�5�   "   �� $ 
runSender0��6)    6Q   �� *" Model.LastMsg.Sender.DisplayName+$ OneWay=� � ��� 5�   "   $ 
runSender1��6)    � �$ :��6Q   5�      �� 5�       6*   �� *Model.LastMsg.Sender+$ OneWay=� 6_   Z� .a�5�   "   �� $ 
runSender0��6)    � �$  ��6Q   �� 5�   "   $ 
runSender1��6)    � �$  ��6Q   5�      �� 5�      $ false��6b    6*   �� *Model.Chat.IsNotDisturb+$ OneWay=�.a�5�   "   �� $ tbUnReadCount��6)    � y�$ 	Collapsed=�6Z   5�      ^� 5�       5�   "   �� 5�   &    60   �� *Model.Chat.IsNotDisturb+$ OneWay=� true6h   �� 5�   &    60   �� *UnReadCount+ 6P   Z� .0�5�   "   �� $ tbUnReadCount��6)    � y�$ 	Collapsed=�6Z   5�      ^� 5�       5�   "   �� 5�   &    60   �� *Model.Chat.IsNotDisturb+$ OneWay=� true6h   �� 5�   &    60   �� *UnReadCount+ 99+6P   .0�5�   "   �� $ runSenderLeft��6)    � �$ [��6T   �� 5�   "   $ runSenderMiddle��6)    � �$ 99��6V   �� 5�   "   $ runSenderRight��6)    � �$
 条]��6U   5�      ^� 5�       5�   "   �� 5�   &    60   �� *Model.Chat.IsNotDisturb+$ OneWay=� true6h   �� 5�   &    60   �� *UnReadCount+  6P   .0�5�   "   �� $ tbUnReadCount��6)    � y�$ 	Collapsed=�6Z   5�      ^� 5�       5�   "   �� 5�   &    60   �� *Model.Chat.IsNotDisturb+$ OneWay=� true6h   �� 5�   &    60   �� *UnReadCount+ 06P   .0�5�   "   �� $ tbUnReadCount��6)    � y�$ 	Collapsed=�6Z   5�      P� 5�       �@J�IsMouseOver . . �5�   "   �� 8 
  � ��6?   5�      P� 5�       �@��
IsSelected . . �5�   "   �� 8   � ��6?   5�      ^� 5�       5�   "   �� 5�   &    60   �� *	IsGroup+$ OneWay=� true6X   �� 5�   &    60   �� *Model.Chat.DisplayName+  6[   .0�5�   "   ��  � y�$ 	Collapsed=�6?   �� 5�   "   $ tbDisplayName��6)   8   � ��6Z   5�      5�      5�      5�      5�      .��5�      � .+�5�   
   �� -   5         ��Name$
 list��5�       %� 
ScrollUnit$ Pixel=�5�      ��5�       �� *Items+$ OneWay=�5�      5�      5�      