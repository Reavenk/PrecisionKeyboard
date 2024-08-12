mergeInto(LibraryManager.library, 
{
    BrowserTextDownload: function(filename, textContent)
    {
        // https://ourcodeworld.com/articles/read/189/how-to-create-a-file-and-generate-a-download-with-javascript-in-the-browser-without-a-server
        var strFilename = Pointer_stringify(filename);
        var strContent = Pointer_stringify(textContent);

        var element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(strContent));
        element.setAttribute('download', strFilename);
        element.style.display = 'none';
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    },

    BrowserTextUpload: function(extFilter, gameObjName, dataSinkFn)
    {
        if(typeof inputLoader == "undefined")
        {
            inputLoader = document.createElement("input");
            inputLoader.setAttribute("type", "file");
            inputLoader.style.display = 'none';
            document.body.appendChild(inputLoader);

            inputLoader.onchange = 
                function(x)
                {
                    if(this.value == "")
                        return;

                    var file = this.files[0];
                    var reader = new FileReader();
                    this.value = "";
                    var thisInput = this;
                    reader.onload = function(evt) 
                    {
	                    if (evt.target.readyState != 2)
		                    return;

	                    if (evt.target.error) 
	                    {
		                    alert("Error while reading file " + file.name + ": " + loadEvent.target.error);
		                    return;
	                    }
                        gameInstance.SendMessage(
                            thisInput.gameObjName, 
                            thisInput.dataSinkFn, 
                            evt.target.result);
                    }
                    reader.readAsText(file);
                }
        }
        inputLoader.gameObjName = Pointer_stringify(gameObjName);
        inputLoader.dataSinkFn = Pointer_stringify(dataSinkFn);
        inputLoader.setAttribute("accept", Pointer_stringify(extFilter))
        inputLoader.click();
    },

    BrowserAlert : function(msg)
    {
        var msg = Pointer_stringify(msg);
        alert(msg);
    },
	
	BrowserGetLinkSearch : function()
	{
		// https://css-tricks.com/snippets/javascript/get-url-and-url-parts-in-javascript/
		var search = window.location.search;
		var searchLen = lengthBytesUTF8(search) + 1;
		var buffer = _malloc(searchLen);
		stringToUTF8(search, buffer, searchLen);
		return  buffer;
	},
	
	BrowserGetLinkHREF : function()
	{
		// https://css-tricks.com/snippets/javascript/get-url-and-url-parts-in-javascript/
		var search = window.location.href;
		var searchLen = lengthBytesUTF8(search) + 1;
		var buffer = _malloc(searchLen);
		stringToUTF8(search, buffer, searchLen);
		return  buffer;
	},

    DoThat : function(val)
    {
        alert("Do that!");
        //obj.DestroySelf();
    },
});