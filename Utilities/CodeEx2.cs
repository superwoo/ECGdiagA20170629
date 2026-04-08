/****************************************************************************
 File:	CodeEx2.cs
 Version:	1.0
 Created:	Mar.9,2010 (Converted to C# for .NET 10: 2026)

 Author:	HuSheping
 E-mail:	hspecg@163.com

 Function:	Linked list management for Minnesota codes and analysis results
	(1) class CodeMgr: Minnesota code management
	(2) Converted from C++ to C# for .NET 10

 Copyright (c) 2010 HuSheping
 PLEASE LEAVE THIS HEADER INTACT
****************************************************************************/

namespace ECGDiag.Utilities;

/// <summary>
/// Code structure containing diagnostic code information
/// </summary>
public struct Code
{
    public ushort NCode;        // 编码
    public uint NLeads;         // 涉及的导联
    public byte NClass;         // 级别
    public byte NSort;          // 排序级别,按两位数,数小的排在前面
    public ushort NIndex;       // 解释索引
    public string SzCse;        // CSE编码

    public Code()
    {
        NCode = 0;
        NLeads = 0;
        NClass = 0;
        NSort = 0;
        NIndex = 0;
        SzCse = string.Empty;
    }
}

/// <summary>
/// Linked list node for code management
/// </summary>
internal class CodeListNode
{
    public Code Code { get; set; }
    public CodeListNode? Link { get; set; }

    public CodeListNode()
    {
        Code = new Code();
        Link = null;
    }
}

/// <summary>
/// Code management class using linked list
/// 编码单项链表管理类
/// </summary>
public class CodeMgr
{
    private CodeListNode? _head;
    private CodeListNode? _current;
    private Code[]? _codes;
    private int _codesCount;

    public CodeMgr()
    {
        _head = null;
        _current = null;
        _codes = null;
        _codesCount = 0;
    }

    ~CodeMgr()
    {
        Reset();
    }

    /// <summary>
    /// Get all codes as an array
    /// 最后结果检索
    /// </summary>
    /// <param name="count">Number of codes</param>
    /// <returns>Array of codes</returns>
    public Code[]? GetCodes(out int count)
    {
        count = GetCount();
        if (count < 1) return null;

        if (_codes != null && count != _codesCount)
        {
            _codes = null;
            _codesCount = count;
        }

        _codes ??= new Code[count];

        int i = 0;
        CodeListNode? r = GetFirst();
        while (r != null)
        {
            _codes[i].NLeads = r.Code.NLeads;
            _codes[i].NCode = r.Code.NCode;
            _codes[i].NClass = r.Code.NClass;
            _codes[i].NSort = r.Code.NSort;
            _codes[i].SzCse = r.Code.SzCse;
            i++;
            r = GetNext();
        }
        return _codes;
    }

    /// <summary>
    /// Insert a code after specified node
    /// 在指定的节点后插入一项
    /// </summary>
    /// <param name="p">Node to insert after</param>
    /// <param name="code">Code to insert</param>
    public void Insert(CodeListNode? p, Code code)
    {
        var q = new CodeListNode { Code = code };

        if (_head == null)
        {
            _head = q;
            q.Link = null;
        }
        else if (p != null)
        {
            q.Link = p.Link;
            p.Link = q;
        }
    }

    /// <summary>
    /// Add a code at the end of the list
    /// 在表尾添加一项
    /// </summary>
    /// <param name="code">Code to add</param>
    public void Add(Code code)
    {
        var q = new CodeListNode { Code = code, Link = null };

        if (_head == null)
        {
            _head = q;
        }
        else
        {
            var p = _head;
            while (p.Link != null)
            {
                p = p.Link;
            }
            p.Link = q;
        }
    }

    /// <summary>
    /// Remove a code from the list
    /// 删除指定分析结果码
    /// </summary>
    /// <param name="nCode">Code to remove</param>
    public void Remove(ushort nCode)
    {
        if (_head == null) return;

        if (_head.Code.NCode == nCode)
        {
            _head = _head.Link;
            return;
        }

        var q = _head;
        var p = _head.Link;

        while (p != null && p.Code.NCode != nCode)
        {
            q = p;
            p = p.Link;
        }

        if (p != null)
        {
            q.Link = p.Link;
        }
    }

    /// <summary>
    /// Remove all items and clear memory
    /// 删除所有项,并清理内存
    /// </summary>
    public void Reset()
    {
        _head = null;
        _current = null;
        _codes = null;
        _codesCount = 0;
    }

    /// <summary>
    /// Find a code in the list
    /// 找到指定分析结果码的项
    /// </summary>
    /// <param name="nCode">Code to find</param>
    /// <returns>Node containing the code, or null if not found</returns>
    public CodeListNode? Found(ushort nCode)
    {
        var p = _head;
        while (p != null && p.Code.NCode != nCode)
        {
            p = p.Link;
        }
        return p;
    }

    /// <summary>
    /// Get count of items in the list
    /// 查找列表中的项数
    /// </summary>
    /// <returns>Number of items</returns>
    public int GetCount()
    {
        int count = 0;
        var p = _head;
        while (p != null)
        {
            count++;
            p = p.Link;
        }
        return count;
    }

    /// <summary>
    /// Get first node in the list
    /// 得到表中第一项节点指针
    /// </summary>
    /// <returns>First node, or null if empty</returns>
    public CodeListNode? GetFirst()
    {
        _current = _head;
        return _head;
    }

    /// <summary>
    /// Get next node in the list
    /// 得到表中下一项节点指针
    /// Note: Must call GetFirst() before calling this method
    /// </summary>
    /// <returns>Next node, or null if at end</returns>
    public CodeListNode? GetNext()
    {
        if (_current != null)
        {
            _current = _current.Link;
        }
        return _current;
    }

    /// <summary>
    /// Replace a code with a new code
    /// 替代检测到的码
    /// </summary>
    /// <param name="nOldCode">Code to replace</param>
    /// <param name="nNewCode">New code</param>
    /// <param name="nLeads">New leads mask</param>
    /// <returns>Resulting code number</returns>
    public ushort Replace(ushort nOldCode, ushort nNewCode, uint nLeads)
    {
        ushort code = nOldCode;
        var p = Found(nOldCode);
        if (p != null)
        {
            if (nOldCode != nNewCode)
            {
                var updatedCode = p.Code;
                updatedCode.NCode = nNewCode;
                p.Code = updatedCode;
                code = nNewCode;
            }
            var updatedCode2 = p.Code;
            updatedCode2.NLeads = nLeads;
            p.Code = updatedCode2;
        }
        return code;
    }

    /// <summary>
    /// Get information about a code
    /// 查询检测到的码信息
    /// </summary>
    /// <param name="nCode">Code to query</param>
    /// <param name="nLeads">Leads mask (input/output)</param>
    /// <returns>-1: code not found, 0: no specific leads, >0: number of leads involved</returns>
    public short GetInfo(ushort nCode, ref ushort nLeads)
    {
        var m = Found(nCode);
        if (m == null) return -1;

        nLeads &= (ushort)m.Code.NLeads;
        if (nLeads == 0) return 0;

        short n = 0;
        uint check = 1;
        for (short i = 0; i < 12; i++)
        {
            if (i > 0) check <<= 1;
            if ((nLeads & check) != 0) n++;
        }
        return n;
    }
}
