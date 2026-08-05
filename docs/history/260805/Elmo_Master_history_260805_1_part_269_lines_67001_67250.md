>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67005 chars=9977 sha256=3773e74f9a6544cb2d1a667a8958a899b491cde2d5194f58ca18f5163ead92f7]'... 44231 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDR8aWmiWmu3c+owajJLeX12zSQXCKsYW4deEMZ3cDONwz6iqNv4Ys4NQSDUZp3iW5uI5HgIUtHHCJFZQQcE575rovFdhe3OtXiz+ENbvTbXt00Ese9YJVeZ3BZRGSV5/hcZHQisiO38Ubi9x4Z1aSVpriZmFpIoJliEeANvAGM/pxX21KpL2atLo+q+VtdPwPiqlOPtXzR0v2fzvprr6mPLo9rpwv5b/zrmG3nhWP7PKI/OikSRlcMVbGQqHp3I+nqXwjtNOsPGOqWumRXaBLNGka4nWXdu2MMbUXGMkd88dO/n0mm+JZfDS6VL4Z1ZnSVXSf7LJkIu8hMbefmkY5z3I+nofwitdU/4SzVr7UtJvdPSW0jjX7REyglQq8EgZ+7muHOJOWFleV9Y9f70en3s7coio4qNo20fTb3ZaX+71+89VlNZty3BrRm6VmXXSvnqZ9FMybuQjNZjuWfAIHU5JwAPWr15nmswt5cysRkA8g9x3Fd8FpocU3rqVtOv7bU4fO0q8tr+EdXtZRIB9cdPxrQt5Tnmvk+HwJr0Pxl1PQPDd4+nzQSvOt2sjII7c/MrEryflZRjueK+n9Atbqx0m0tNR1OXVbyIESXsqBGlyeOB2HTnmsqVWVTRo0qUow1TOntH6VsWzdKw7MdK2bXtWdVGlJmlEanFV4qsLXIzpQtFFFSUFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAf/Z',
>       originX: 1398,
>       originY: 747,
>       width: 209,
>       height: 16
>     }
>   ],
>   accessibility: null
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.type_text({window:targetWindow,text:'UDINT'}); nodeRepl.write('ok');",
>   "title": "Set UDINT type"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "Inspect UDINT match"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67070 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 315863 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67079 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 308935 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67088 chars=9544 sha256=c06f153736cea96063f5273bcf70bf1936c55e1b337b827fb2e8df77d4ccacab]',
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD1XwB4iu4PCHxBlSOAtomt6uLcFTh8SyTfPzz8zkcY4x35rch1LxXquqaquj3WhwwWJhVbe6spXaZmhSQgyrMAgJfGfLbHXDdKwfDPgnVpNN8RQx6l/Z2n63quovf2s9iWmZWu5kDwyb12b4RHglXHRh1rp5vDWtxaxqc2ka7aWFhqDRtJGNOMlxFtjWP93KZdgOFBG6NgD2NVNpybjoPt/Xb/AIJm3fjXUbvSJLnRba1juJLOwmgjugzBJZ7homVyrDIXb1GOcnkVoJ4hv9Z/s2HRWtbGe8s7iVzeQNP9nmhliR42VZEzgvIpw3UA5wMGxL4PgGEtLgw26Q2MKIULkC2nMvLE5JbOM9uvPSpofDS2/jKXXYLnZFLbvHJa7ODKxjzKGzxlYkBGO2fXKVr6+f5af1/wRN9vL8l+v6lHwNfa3qej/b9du9Nm84sIks7N4Nm12U7i0r7s4B4xjnrWrctwaboWl/2No0Nj53neWznft253OW6ZPrii66VtDUxmZN3IRmsx3LPgEDqck4AHrV68zzWYW8uZWIyAeQe47iu+C00OKb11K2nX9tqcPnaVeW1/COr2sokA+uOn41oW8pzzXyfD4E16H4y6noHhu8fT5oJXnW7WRkEdufmViV5Pysox3PFfT+gWt1Y6TaWmo6nLqt5ECJL2VAjS5PHA7DpzzWVKrKpo0aVKUYapnT2j9K2LZulYdmOlbNr2rOqjSkzSiNTiq8VWFrkZ0oWiiipKCiiigAooooAKKKKACiiigAooooAKKKKAP//Z',
>       originX: 1398,
>       originY: 747,
>       width: 209,
>       height: 16
>     }
>   ],
>   accessibility: null
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:targetWindow,key:'Down'}); nodeRepl.write('ok');",
>   "title": "Select UDINT match"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "Inspect UDINT selection"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67153 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 315863 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67162 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 308935 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67171 chars=9544 sha256=c06f153736cea96063f5273bcf70bf1936c55e1b337b827fb2e8df77d4ccacab]',
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD1XwB4iu4PCHxBlSOAtomt6uLcFTh8SyTfPzz8zkcY4x35rch1LxXquqaquj3WhwwWJhVbe6spXaZmhSQgyrMAgJfGfLbHXDdKwfDPgnVpNN8RQx6l/Z2n63quovf2s9iWmZWu5kDwyb12b4RHglXHRh1rp5vDWtxaxqc2ka7aWFhqDRtJGNOMlxFtjWP93KZdgOFBG6NgD2NVNpybjoPt/Xb/AIJm3fjXUbvSJLnRba1juJLOwmgjugzBJZ7homVyrDIXb1GOcnkVoJ4hv9Z/s2HRWtbGe8s7iVzeQNP9nmhliR42VZEzgvIpw3UA5wMGxL4PgGEtLgw26Q2MKIULkC2nMvLE5JbOM9uvPSpofDS2/jKXXYLnZFLbvHJa7ODKxjzKGzxlYkBGO2fXKVr6+f5af1/wRN9vL8l+v6lHwNfa3qej/b9du9Nm84sIks7N4Nm12U7i0r7s4B4xjnrWrctwaboWl/2No0Nj53neWznft253OW6ZPrii66VtDUxmZN3IRmsx3LPgEDqck4AHrV68zzWYW8uZWIyAeQe47iu+C00OKb11K2nX9tqcPnaVeW1/COr2sokA+uOn41oW8pzzXyfD4E16H4y6noHhu8fT5oJXnW7WRkEdufmViV5Pysox3PFfT+gWt1Y6TaWmo6nLqt5ECJL2VAjS5PHA7DpzzWVKrKpo0aVKUYapnT2j9K2LZulYdmOlbNr2rOqjSkzSiNTiq8VWFrkZ0oWiiipKCiiigAooooAKKKKACiiigAooooAKKKKAP//Z',
>       originX: 1398,
>       originY: 747,
>       width: 209,
>       height: 16
>     }
>   ],
>   accessibility: null
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:targetWindow,key:'Return'}); nodeRepl.write('ok');",
>   "title": "Commit UDINT type"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "Verify UDINT input"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67236 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 363571 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67245 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 352707 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     }
