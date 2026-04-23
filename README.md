# DeadlockTools
This tool has a few utility functions. It reduces time spent on switching SDKs (in fact it makes the CS2 SDK not needed at all for model replacements).

This is also incredibly useful for those on Linux. As the CS2 SDK does not run on Linux in my experience.

## HOW TO USE
The easiest way to use this is to write a small shell/batch file to quickly run a lot of commands.

<details>
<summary>Example shell script</summary>

```
#!/bin/sh

VMDL_PATH="/mod/models/heroes_wip/mcginnis/mcginnis.vmdl_c"

./DeadlockTools add ag2 $VMDL_PATH -h mcginnis
./DeadlockTools fix unitstatus $VMDL_PATH
```
</details>

## COMMANDS

<details>
<summary>add ag2</summary>

`add ag2` is the command to add AnimGraph2 and Skeleton references to your model.
This command overwrites the file provided.

The format looks like this:

`add ag2 <model> -h <hero>`

| Parameter | Explanation |
| -- | -- |
| `<model>` | The path to your `vmdl_c` file. Can be relative or absolute. |
| `-h <hero>` | The hero's internal name. (Doesn't always match the character's name. Ex: Mina is `vampirebat`) |
| `-f <hero_folder>` | The folder the hero belongs to. Typically `heroes_wip` or `heroes_staging`. Defaults to wip. |
| `--override-skeleton <path>` | Full skeleton path override for heroes with unusual file structures. |

For example, to add the needed references for McGinnis, this command can be used.

`DeadlockTools.exe add ag2 ./path/to/model.vmdl_c -h mcginnis`

For Ivy.

`DeadlockTools.exe add ag2 ./path/to/model.vmdl_c -h ivy --override-skeleton -f heroes_staging "models/heroes_staging/tengu/tengu_v2/dmx/mesh/ivy.vnmskel"`

</details>

<details>
<summary>fix unitstatus</summary>

`fix unitstatus` is the command to fix the unit status data for models compiled in CSDK12. The game expects a single object but the data gets compiled as an array of objects. This command overwrites the file provided.

The format looks like this:

`fix unitstatus <model>`

| Parameter | Explanation |
| -- | -- |
| `<model>` | The path to your `vmdl_c` file. Can be relative or absolute. |

Example:

`DeadlockTools.exe fix unitstatus ./path/to/model.vmdl_c`

</details>

<details>
<summary>print</summary>

`print` is a debug command. It just prints out the entire DATA block for a model file.

</details>

## KNOWN ISSUES
The ValveResourceFormat library marks its modification functions as experiemental.

These side effects may occur:
- Larger file size
    
    My model files have grown to double the size with this tool. I believe this is due to some form of padding done by the library. However the amount of growth may be different for most users as my models are rather low poly.
