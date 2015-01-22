Awaitable [![Build status](https://ci.appveyor.com/api/projects/status/i6ngcmla8kyql8u8?svg=true)](https://ci.appveyor.com/project/StevenThuriot/awaitable)
=========

Awaitable is a Framework around `async`/`await`, letting you use it without having it spread like a virus.

`Async`/`await` is a pretty nice solution Microsoft has presented when trying to quickly build multithreaded app. For larger apps, I personally prefer to hold a tighter control over what is happening and rather use the TPL (`Tasks`) directly.

There is one major drawback, though. Using it feels like a virus spreading through your code, quickly infecting all of your methods. You'll quickly notice you're adding the `async` keyword to your methods all the way up to your domain if you're not careful.

Awaitable is a solution for this problem. By wrapping an instance using Awaitable, you'll be able to `await` synchronous methods. These are automatically turned into tasks. Methods that are already `async` will be left alone and executed as is. That way you don't have to double `await` them.


#Usage

Silly sample class

```csharp
public class SynchronousModel()
{
  public string ReadFileFromDisk(bool includeA = true, bool includeB = false, bool readToEnd = true)
  {
    //Stuff
  }
}
```

The boring usual thing
----------------------

```csharp
//Making a new synchronous model
var model = new SynchronousModel();

//Executed synchronously, the usual situation.
string result = model.ReadFileFromDisk();
```

Awaitable in action
-------------------

```csharp
//Making our model async
var asyncModel = model.Awaitable();

//Executed asynchronously!!
string result = await asyncModel.ReadFileFromDisk();

//Or with parameters
string result = await asyncModel.ReadFileFromDisk(true, true, true);

//Named parameters work too! 
//Parameters that aren't passed to the method, will use their default value if defined.
string result = await asyncModel.ReadFileFromDisk(readToEnd: false); 

//Mixes work fine as well!
string result = await asyncModel.ReadFileFromDisk(true, readToEnd: false); 

//Properties? No problem!
asyncModel.Awesomeness = 9006;

//You can unwrap it again with a simple cast.
SynchronousModel unwrapped = asyncModel;

//Or any other cast your class supports
string classAsString = asyncModel;
```
