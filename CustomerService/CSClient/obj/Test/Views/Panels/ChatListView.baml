   M S B A M L   `   `   `  ���� C  ?CSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:CSClient.ConverterCSClient.Converter  (    "CSClient.Views.Panels.ChatListView   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     /cv clr-namespace:CSClient.Converter       � 	Resources      BooleanToVisibilityConverter2  ,CSClient.Converter.DataTimeToStringConverter  dts4   .CSClient.Converter.UnReadMessageCountConverter  	unreadMsg1   +CSClient.Converter.ModifyGroupNameConverter  ModifyGroupNameConverter'  !CSClient.Converter.HeadImgConvter  htc �� 
TargetType �� Property �� Value �� 
TargetType ��Name F�Name �Name � ContextMenu	 m�Name
 m� Command � RowDefinitions � ColumnDefinitions �� ImageSource �� Mode �� 	Converter ��Name  CommonFontSize � ToolTip �� RelativeSource  LowerFontSize ��Name  LightForeground ��Name ��Name  AppendLabelStyle �� Triggers �� Value �� Binding �� 
TargetName ^� 
Conditions �� Binding �� Value P� Property P� Value 	 MouseOverBackground 
 SelectedBackground��%�  &	        &	      &	 
     &	      &	      '��      7  7  7  7  7   7  7  7  7  7  7  7  7  7  7  7 	 7 
 7  ��         �� " ��.���  �@�FocusVisualStyle Z� ��  � ��$ 0q���  � �� Z� ��  � ��
 ���� ��  � �� �� " ��.R��� $ bd��#��z��$��0,0.5q�._�� .+�F� $ pathTopMost��$y�	Collapsed=�"����        �    ��� р��     	$��5q�$
��Left=�$	��Top=����#688FFF� $
 grid��-   $��50��$
��5,10q�$	��220��
������  �� .@�m� $	 miIsTopMost��$y�	Collapsed=�$��置顶��
 �� *TopMostCommand+m� $	 miIsNotDisturb��$y�	Collapsed=�$��消息免打扰��
 �� *NoDisturbCommand+m� $��删除聊天��
 �� *DeleteChatCommand+ �� ��  �� $��50 ��� �� $
��Auto �.+�A� $��2��$��6q�����  �� *Model.Chat.HeadImg+$ TwoWay=�8   �� $ tbDisplayName��$��1��$	��130��$��CharacterEllipsis=����#333333$
��Left=�$��Center=�8�� ���� *Model.Chat.DisplayName+$ OneWay=� �� *Text+ �� *Self+$ OneWay=��� $��2��$��Center=����#9999998�� ���� *Model.LastMsg.SendTime+8  $ OneWay=�]� $��1��$��1��$��2����. $��Center=�.+��� $ imgNodisturb��$y�	Collapsed=�$3��-/IMAssets;component/Images/icon-nodisturb.png��$��Right=�$��12��$��12��$��10,0,0,0q��� $��Right=�$��会话已结束��$��0,0,10,0q����#999999$��12(�y��� *IsSessionEnd+8  �� $
 tbAt��8�� 8�� $
��Left=��� $ tbStartSession��8�� 8�� $
��Left=��� $ tbUnReadCount��$
��Left=�8��	 8��
 �� $ runSenderLeft��$�[�� �� $ runSenderMiddle����� *UnReadCount+8  $ OneWay=� �� $ runSenderRight��$
�条]���� $��CharacterEllipsis=�8�� ���#999999�� $ 
runSender0�� �� $ 
runSender1�� �� ��� *Model.LastMsg.Content+$ OneWay=��� $ lblUnReadCount��8�� $��Right=�$	��Top=�
������ $��	0,10,-5,0q�$��2������ *Model.Chat.IsNotDisturb+���� *UnReadCount+8  $ OneWay=� �� $
 true�� �� *IsSessionEnd+.a��� $ tbDisplayName�� � �� �#999999�� $
 true�� �� *	IsGroup+$ OneWay=�.a��� $ 
runSender0�� �� *" Model.LastMsg.Sender.DisplayName+$ OneWay=� � ��� $ 
runSender1�� � �$ :����  �� *Model.LastMsg.Sender+$ OneWay=� Z� .a��� $ 
runSender0�� � �$  ���� $ 
runSender1�� � �$  ���� $
 true�� �� *Model.Chat.IsNotDisturb+$ OneWay=�.a��� $ imgNodisturb�� � y�$ Visible=��� $ miIsNotDisturb�� � ��$ 开启新消息提醒���� $
 true�� �� *
HasAtMsg+.a��� $
 tbAt�� � ��$ [@我]���� $
 tbAt�� � y�$ Visible=��� $
 tbAt�� � ��
 �  ���� $
 true�� �� *IsDisplayStartSession+.a��� $ tbStartSession�� � ��$ [接入聊天]���� $ tbStartSession�� � y�$ Visible=��� $ tbStartSession�� � ��
 �  ���� $ notification�� �� *Model.LastMsg.MsgType+$ OneTime=�.a��� $ 
runSender0�� � �$  ���� $ 
runSender1�� � �$  ���� $ false�� �� *Model.Chat.IsNotDisturb+$ OneWay=�.a��� $ tbUnReadCount�� � y�$ 	Collapsed=�^�  ��  �� *Model.Chat.IsNotDisturb+$ OneWay=� true��  �� *UnReadCount+ Z� .0��� $ tbUnReadCount�� � y�$ 	Collapsed=�^�  ��  �� *Model.Chat.IsNotDisturb+$ OneWay=� true��  �� *UnReadCount+ 99+.0��� $ runSenderLeft�� � �$ [���� $ runSenderMiddle�� � �$ 99���� $ runSenderRight�� � �$
 条]��^�  ��  �� *Model.Chat.IsNotDisturb+$ OneWay=� true��  �� *UnReadCount+  .0��� $ tbUnReadCount�� � y�$ 	Collapsed=�^�  ��  �� *Model.Chat.IsNotDisturb+$ OneWay=� true��  �� *UnReadCount+ 0.0��� $ tbUnReadCount�� � y�$ 	Collapsed=��� $
 true�� �� *Model.Chat.IsTopMost+$ OneWay=�.a��� $ miIsTopMost�� � ��$ 取消置顶���� $ pathTopMost�� � y�$ Visible=�P�  �@J�IsMouseOver . . ��� 8   � ��P�  �@��
IsSelected . . ��� 8   � ��^�  ��  �� *	IsGroup+$ OneWay=� true��  �� *Model.Chat.DisplayName+  .0���  � y�$ 	Collapsed=��� $ tbDisplayName��8   � ��.��� .+��� -    ��Name$
 list��  %� 
ScrollUnit$  Pixel=����� *Items+$ OneWay=�! �� SelectedItem! �� *SelectedItem+$ TwoWay=��� -   $ searchResults��  IntervalBackground#��[ ���� *SearchResults+$ OneWay=�! �� *SelectedSearchItem+$ TwoWay=�y��� *SearchVisibility+$ OneWay=�