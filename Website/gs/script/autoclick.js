<!--
function fnTrapKD(btn, evnt)
{ /* Invoke click event of button if user presses Enter key */
	if (evnt.keyCode == 13)
	{
		evnt.returnValue=false;
		evnt.cancel = true;
		clickElement(btn.id);
	}
}

function clickElement (elementId)
{ /* Invoke click event of element */
	var element;
	if (typeof document.getElementById !== 'undefined') {
		element = document.getElementById(elementId);
		if (typeof document.createEvent !== 'undefined' &&
				typeof element.dispatchEvent !== 'undefined') {
			var event = document.createEvent('MouseEvents');
			event.initMouseEvent(
				'click',
				true,
				true,
				document.defaultView,
				1,
				0,
				0,
				0,
				0,
				false,
				false,
				false,
				false,
				0,
				null
			);
			element.dispatchEvent(event);
		}
		else if (typeof element.click !== 'undefined') {
			element.click();
		}
	}
}
-->