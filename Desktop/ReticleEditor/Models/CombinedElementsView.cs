using BallisticCalculator.Reticle.Data;
using System.Collections;
using System.Collections.Generic;

namespace ReticleEditor.Models;

/// <summary>
/// Provides a combined read-only view of ReticleElements and BDC points
/// for display in a ListBox without copying the collections.
/// Works like WinForms virtual list - elements call ToString() when displayed.
/// </summary>
public class CombinedElementsView : IReadOnlyList<object>
{
    private readonly ReticleElementsCollection _elements;
    private readonly ReticleBulletDropCompensatorPointCollection _bdcPoints;

    public CombinedElementsView(ReticleDefinition reticle)
    {
        _elements = reticle.Elements;
        _bdcPoints = reticle.BulletDropCompensator;
    }

    public int Count => _elements.Count + _bdcPoints.Count;

    public object this[int index]
    {
        get
        {
            if (index < _elements.Count)
                return _elements[index];
            return _bdcPoints[index - _elements.Count];
        }
    }

    public IEnumerator<object> GetEnumerator()
    {
        foreach (var element in _elements)
            yield return element;
        foreach (var bdc in _bdcPoints)
            yield return bdc;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
