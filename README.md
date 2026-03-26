# DEADLOCK TOOL
This tool has exactly **ONE** function. It lets you add animgraph 2 references and skeletons without the cs2 sdk. i dont feel like switching back and forth between sdks.

heres how to use it. run something like this in your cli.
`./DeadlockTools.exe add ag2 mcginnis.vmdl_c -h mcginnis`

this command opens the tool and runs the `add ag2` command.
- the first option is the target file path (so `mcginnis.vmdl_c` or `vampirebat.vmdl_c`) IT MUST BE A COMPILED MODEL
- the `-h` parameter is the hero id (`mcginnis` or `vampirebat`)

the tool will overwrite the vmdl_c file. make sure to verify that the animgraph refs and skeletons have been added.

## known issues
the valve resource format library marks the export function as experiemental.

these side effects may occur:
- larger file size
