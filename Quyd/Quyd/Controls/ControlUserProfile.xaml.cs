﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Quyd.Controls
{
    public partial class ControlUserProfile : UserControl
    {
        public ControlUserProfile()
        {
            InitializeComponent();
        }
        
        public void setUsername(string username)
        {
            usernameBox.Text = username;
        }

        public void setProfilePicture()
        {

        }
    }
}
