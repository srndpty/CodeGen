using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

/// <summary>
/// ブリッジの名前空間
/// </summary>
namespace DefaultCompany.Test
{
    /// <summary>
    /// {}でインデントを行う
    /// </summary>
    public class CurlyIndent : IDisposable 
    {
        WrappedInt val;
        StringBuilder builder;

        public CurlyIndent(StringBuilder b, WrappedInt counter)
        {
            val = counter;
            builder = b;
            builder.AppendIndentLine(val, "{");
            ++val;
        }

        public void Dispose()
        {
            --val;
            builder.AppendIndentLine(val, "}");
        }
    }

    public class WrappedInt
    {
        int value;

        public WrappedInt Store(int num)
        {
            value = num;
            return this;
        }

        public int Load()
        {
            return value;
        }

        public static implicit operator WrappedInt(int val)
        {
            return new WrappedInt().Store(val);
        }

        public static implicit operator int(WrappedInt val)
        {
            return val.Load();
        }

        public static WrappedInt operator ++(WrappedInt self)
        {
            self.value++;
            return self;
        }

        public static WrappedInt operator --(WrappedInt self)
        {
            self.value--;
            return self;
        }

    }
}
