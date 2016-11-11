# LightMapper
[![Build status](https://ci.appveyor.com/api/projects/status/ysws54hok4gd87ky/branch/master)](https://ci.appveyor.com/project/bearpawmaxim/lightmapper/branch/master)
[![Test status](http://teststatusbadge.azurewebsites.net/api/status/bearpawmaxim/LightMapper)](https://ci.appveyor.com/project/bearpawmaxim/lightmapper/branch/master/tests)

## Features
A small mapper with a base set of functionality, that:
- [x] Is easy to set up and use;
- [x] Performs mapping as fast as lightning;
- [x] Available on Nuget as [LightMapper](https://www.nuget.org/packages/LightMapper/);
- [x] Uses Reflection.Emit for creation of mapping methods dynamically;
- [x] Can map properties/fields from **SourceT** class type to **TargetT** class type;
- [x] Have an ability to set explicit property/field to property/field mapping _(eg. TargetClass.DiffField2 = SourceClass.DiffField1)_;
- [x] Can execute explicit **Action<SourceT, TargetT>** before/after mapping;
- [x] Can execute explicit actions of **TargetT** base class;
- [x] Can create target class via func (for classes with non-parameterless constructors);
- [x] Can map enums to their underlying types (int in most cases) and vice versa;
- [ ] Can map nested classes (in progress, can be done with help of explicit actions);

## Examples
### Initialization & Mapping creation
```C#
var mapper = LightMapper.Instance; // or new LightMapper(); 

// Creates a mapping from SourceClass to TargetClass
// including properties and fields
var mappingItem = _mapper.CreateMapping<SourceClass, TargetClass>(true);
// Registers a mapping
mapper.AddMapping(mappingItem);
```

### Setting custom target type constructor
```C#
// // Creates a mapping from SourceClass to TargetClass
var mappingItem = _mapper.CreateMapper<SourceClass, TargetClass>(true)
	// sets a custom constructor
	.SetConstructorFunc(() => new TargetClass());
// Registers a mapping
mapper.AddMapping(mappingItem);
```

### Setting Explicit actions and fields
```C#
// Creates a mapping from SourceClass to TargetClass
var mappingItem = _mapper.CreateMapper<SourceClass, TargetClass>(true)
    // sets BoolProp to true if DateTimeProp value is older than the week
    .Explicit((src, trg) => trg.BoolProp = src.DateTimeProp < DateTime.Now.AddDays(-7))
    // sets that the DiffField1 of SourceClass will be mapped into DiffField2 of TargetClass
    .ExplicitMember(t => t.DiffField2, s => s.DiffField1);
// Registers a mapping (LightMapper will compile mapping methods)
mapper.AddMapping(mappingItem);
```

### Updating & Deleting a mapping
Updating a mapping
```C#
// gets existing mapping fo SourceClass to TargetClass
var mappingItem = _mapper.GetMapping<SourceClass, TargetClass>();
// sets that the DiffProp1 of SourceClass will be mapped into DiffProp2 of TargetClass
mappingItem.ExplicitMember(t => t.DiffProp2, s => s.DiffProp1);
// updates mapping (LightMapper will recompile mapping methods)
_mapper.UpdateMapping(mappingItem);
```

Removing a mapping
```C#
// removes mapping of SourceClass to TargetClass
var mappingItem = _mapper.RemoveMapping<SourceClass, TargetClass>();
```

### Mapping
Mapping of single instance of SourceClass to TargetClass
```C#
SourceClass src = GetFromDbOrCreateOne();
TargetClass targ = _mapper.Map<SourceClass, TargetClass>(src);
```

Mapping of collection of SourceClass to collection of TargetClass
```C#
IEnumerable<SourceClass> srcList = GetFromDbOrCreateCollection();
IEnumerable<TargetClass> = _mapper.Map<SourceClass, TargetClass>(srcList);
```
