﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ToolGood.Words;

namespace CSClient.Helper
{
    public static class CommonHelper
    {
        public const char SORT_CHAR = '[';
        public const char GROUPBY_CHAR = '#';

        /// <summary>
        /// 获取字符串首个字 的 首个字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static char GetFirstChar(string source, bool isGroupBy = false)
        {
            if (string.IsNullOrEmpty(source))
            {
                return isGroupBy ? GROUPBY_CHAR : SORT_CHAR;
            }

            var t = source.Substring(0, 1);
            if (WordsHelper.HasChinese(t))
            {
                return char.Parse((WordsHelper.GetFirstPinYin(t).Substring(0, 1)));
            }
            else if (WordsHelper.HasEnglish(t))
                return Char.Parse(t.ToUpper());
            else
                return isGroupBy ? GROUPBY_CHAR : SORT_CHAR;
        }

        public static string GetEnumDescription(Enum enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
            if (objs.Length == 0)    //当描述属性没有时，直接返回名称
                return value;
            DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objs[0];
            return descriptionAttribute.Description;
        }
    }
}
