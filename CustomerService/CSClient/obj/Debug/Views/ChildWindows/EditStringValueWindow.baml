   M S B A M L   `   `   `  ����  CSClient7    1CSClient.Views.ChildWindows.EditStringValueWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   $��微软雅黑)�5   F     � ShowInTaskbar  .     ShadowWindowStyle#��[  5       � WindowStartupLocation$ CenterOwner=�6F    � Title$ ChangeGoupNameWin��5   	   $	��200��6#   $	��300��60   .��5      � -   5         � RowDefinitions 5	   
   �� 5
      $
��Auto �6   �� 5      �� 5      $��40 �6   .+�5   
   �� -   5         ��Name$ tbTitle��5      $��标题��6%     MainTitleStyle#��[ 6/   �� 5   
     LineRectStyle#��[ 6   �� 5   
   #��[ 6   $��1��6;   �� 5   
   -   5         ��Name$ tbValue��5      $��1��6#   $��10q�60   �� 5   
   -   5         ��Name$ btnClose��5        CloseButtonStyle#��[ 6#   
 J� Uid$
 Exit��6M   �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   .+�5      �� -   5        $ 	btnCancel��5      $��取消��6(     HightLightButtonStyle#��[ 65   $ Close��6d   $
��10,0q�6q   �� 5      -   5        $ btnSure��5      $��确定��6&   #��[ 63   $
 Sure��6b   $
��10,0q�6m   5      5      5      