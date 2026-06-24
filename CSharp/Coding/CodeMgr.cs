namespace EcgDiag;

/// <summary>
/// 编码结构定义
/// </summary>
public class Code
{
    public ushort nCode;      //编码
    public uint nLeads;       //涉及的导联
    public byte nClass;       //级别
    public byte nSort;        //排序级别
    public ushort nIndex;     //解释索引
    public string szCse = ""; //CSE编码
}

/// <summary>
/// 编码单项链表管理类 - 使用LinkedList实现
/// </summary>
public class CodeMgr
{
    private LinkedList<Code> m_list = new();
    private LinkedListNode<Code>? m_current;
    private Code[]? m_pCode;
    private short m_nCodes;

    public CodeMgr()
    {
        m_current = null;
        m_pCode = null;
        m_nCodes = 0;
    }

    /// <summary>
    /// 在指定的节点后插入一项
    /// </summary>
    public void Insert(LinkedListNode<Code> p, Code code)
    {
        if (m_list.Count == 0)
        {
            m_list.AddFirst(code);
        }
        else
        {
            m_list.AddAfter(p, code);
        }
    }

    /// <summary>
    /// 在表尾添加一项
    /// </summary>
    public void Add(Code code)
    {
        m_list.AddLast(code);
    }

    /// <summary>
    /// 删除指定分析结果码
    /// </summary>
    public void Remove(ushort code)
    {
        var node = m_list.First;
        while (node != null)
        {
            if (node.Value.nCode == code)
            {
                m_list.Remove(node);
                return;
            }
            node = node.Next;
        }
    }

    /// <summary>
    /// 删除所有项
    /// </summary>
    public void Reset()
    {
        m_list.Clear();
        m_current = null;
        m_pCode = null;
        m_nCodes = 0;
    }

    /// <summary>
    /// 找到指定分析结果码的项
    /// </summary>
    public LinkedListNode<Code>? Found(ushort code)
    {
        var node = m_list.First;
        while (node != null)
        {
            if (node.Value.nCode == code) return node;
            node = node.Next;
        }
        return null;
    }

    /// <summary>
    /// 查找列表中的项数
    /// </summary>
    public int GetCount()
    {
        return m_list.Count;
    }

    /// <summary>
    /// 得到表中第一项节点
    /// </summary>
    public LinkedListNode<Code>? GetFirst()
    {
        m_current = m_list.First;
        return m_current;
    }

    /// <summary>
    /// 得到表中下一项节点
    /// </summary>
    public LinkedListNode<Code>? GetNext()
    {
        if (m_current != null) m_current = m_current.Next;
        return m_current;
    }

    /// <summary>
    /// 替代检测到的码
    /// </summary>
    public ushort Replace(ushort nOldCode, ushort nNewCode, uint nLeads)
    {
        ushort code = nOldCode;
        var p = Found(nOldCode);
        if (p != null)
        {
            if (nOldCode != nNewCode) { p.Value.nCode = nNewCode; code = nNewCode; }
            p.Value.nLeads = nLeads;
        }
        return code;
    }

    /// <summary>
    /// 查询检测到的码信息
    /// </summary>
    public short GetInfo(ushort nCode, ref ushort nLeads)
    {
        var m = Found(nCode);
        if (m == null) return -1;

        nLeads &= (ushort)m.Value.nLeads;
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

    /// <summary>
    /// 提取所有的码
    /// </summary>
    public Code[] GetCodes(out int n)
    {
        n = GetCount();
        if (n < 1) { return Array.Empty<Code>(); }

        if (m_pCode == null || n != m_nCodes)
        {
            m_pCode = new Code[n];
            m_nCodes = (short)n;
        }

        int i = 0;
        var r = GetFirst();
        while (r != null)
        {
            m_pCode[i] = new Code
            {
                nLeads = r.Value.nLeads,
                nCode = r.Value.nCode,
                nClass = r.Value.nClass,
                nSort = r.Value.nSort
            };
            i++;
            r = GetNext();
        }
        return m_pCode;
    }
}
