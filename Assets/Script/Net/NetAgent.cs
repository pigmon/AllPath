using System;
using System.Collections;
using System.Collections.Generic;

class NetAgent<TUpDgram>
{
    private Dictionary<int, DGramRcver<TUpDgram>> DictRcver = new Dictionary<int, DGramRcver<TUpDgram>>();

    public void AddRcver(int _vid, int _local_port)
    {
        if (!DictRcver.ContainsKey(_vid))
        {
            DGramRcver<TUpDgram> rcver = new DGramRcver<TUpDgram>();
            rcver.Start(_local_port);
            DictRcver.Add(_vid, rcver);
        }
    }

    public void Dispose()
    {
        foreach (DGramRcver<TUpDgram> rcver in DictRcver.Values)
        {
            rcver.Running = false;
        }
    }

    public void FetchData(int _vid, ref TUpDgram _dgram)
    {
        if (!DictRcver.ContainsKey(_vid))
            return;

        DGramRcver<TUpDgram> rcver = DictRcver[_vid];
        rcver.FetchData(ref _dgram);
    }

    public DateTime LastUpdateOf(int _vid)
    {
        if (!DictRcver.ContainsKey(_vid))
            return DateTime.MinValue;

        return DictRcver[_vid].LastUpdateTime;
    }
}
