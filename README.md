# MessagePack for Unity

## What is it?

This is MessagePack serialization/deserialization for Unity (or Unity Pro.)

This library is based on Kazuki's MessagePack for C#
(not the current official for CLI, because it depends on .NET framework 4.0.) 

## Install

To install this, copy files in src to Assets folder in your project.

## Compiler Symbol

`MSGPACK_UNPACK_DOWNCAST_DOUBLE_TO_FLOAT`

If specified, MessagePack reader will downcast `double` typed value to `float`
if type of awaiting field is `float`.<br/>
This is useful in communication with other MessagePack library of weaker typed
language like `PHP`, which never uses `single` but `double` for floating point
value.


## See also

  Official Library           : https://github.com/msgpack/msgpack
  
  Original C# Implementation : https://github.com/kazuki/msgpack
  
## License

    Copyright (C) 2011-2012 Kazuki Oikawa, Kazunari Kida
    
       Licensed under the Apache License, Version 2.0 (the "License");
       you may not use this file except in compliance with the License.
       You may obtain a copy of the License at
    
           http://www.apache.org/licenses/LICENSE-2.0
    
       Unless required by applicable law or agreed to in writing, software
       distributed under the License is distributed on an "AS IS" BASIS,
       WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
       See the License for the specific language governing permissions and
       limitations under the License.
