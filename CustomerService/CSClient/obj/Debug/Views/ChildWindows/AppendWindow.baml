   M S B A M L   `   `   `  ����C  ?CSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:CSClient.Views.ChildWindowsCSClient.Views.ChildWindows  5   	   .    (CSClient.Views.ChildWindows.AppendWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:CSClient.Views.ChildWindows     5   	   $��微软雅黑)�5   F     � ShowInTaskbar  .   � WindowStyle$
 None=�5       � AllowsTransparency .  � Title$ AppendWindow��5   	   $	��520��6   $	��200��6+   ��6:   Z�  � 	Resources 5	         
OutLoading �� 
TargetName �� TargetProperty W� From	 W� To	 g� Duration  OutUnloading  	InLoading  InUnloading��%"  &	        &	      &	 �     &	 |    �� 5
   
   .��5      W� $ 	translate��6   $ X�6@   $
 -320X�5      $ 0X�6*   $	 0:0:0.25D�61   5      �� 5   
   .��5      W� $ 	translate��6   $ X�6@   $ 0X�5      $
 -320X�6'   $	 0:0:0.15D�62   5      �� 5   
   .��5      W� $ 	translate��6   $ X�6@   $	 320X�5      $ 0X�6)   $	 0:0:0.25D�60   5      �� 5   
   .��5      W� $ 	translate��6   $ X�6@   $ 0X�5      $	 320X�6'   $	 0:0:0.15D�60   5      5      .��5      � -   5        
 �Name$
 
gridLayout��5      z�5   
   T� 5      -   5        	translate��5   !   .+�5    
   �� -   5         ��Name$ rectBG��5         CommonEffect J� Effect# [ 6$   $��10q�6K     AppendBackground#��[ 6W   �� 5!   
   -   5         ��Name$ 	bdContent��5!      $��20q�6%   ._�5"      �� -   5         ��Name$ 	ccContent��5"       � FocusVisualStyle 61   Z� 5#      5$      5'      