MainForm.prototype.NLCreateRoom = function () {
    this.clearServerMessages();

    alert("On MainForm.prototype.NLCreateRoom.");
  
    // Validate input first
    if (!this.ClientSideValidate(false)) {
        return; // Do nothing if basic validation fails
    }

    // get name
    var name = EscapeXmlSensitveChar(Trim(this.tbRoomName.value));

    // get description
    var description = EscapeXmlSensitveChar(this.taRoomDesc.value);

    // get privacy
    var privacy;
    if (this.radOpen.checked)
        privacy = 'Open';
    else if (this.radClosed.checked)
        privacy = 'Closed';
    else
        privacy = 'Secret';

    // get add-in
    var addIn;
    if (this.lbAddIns.selectedIndex == -1)
        addIn = '';
    else
        addIn = this.lbAddIns.options[this.lbAddIns.selectedIndex].value;

    // category
    var cate = this.ChosenCategory();

    // get managers
    var normMans = this.NormalizeNames(this.taManagers.value);
    this.taManagers.value = normMans;
    var managers = EscapeXmlSensitveChar(normMans);

    // get members
    var members = (this.radOpen.checked) ? '' : Trim(this.taMembers.value);
    var normMems = this.NormalizeNames(members);
    this.taMembers.value = normMems;
    members = EscapeXmlSensitveChar(normMems);

    // get notification
    var notify = this.radInherit.checked ? "inherit" : "false";

    // set parameters and progress status
    this.RequestInProgress = RequestInProgress.RM_CreateRoom;
    var params = StringFormat(_RMCommand_CreateRoom, name, description, privacy,
                              addIn, cate, managers, members, notify);
    var reqUserInfo = _requestsHeader + StringFormat(_requestsShell, params);
    window.setTimeout(TimerHandler(this.connection, this.connection.SendHttpRequest,
                    "POST", _RMHandlerUrl, reqUserInfo, this.dataUser.WebTicket), 0);

    this.ToggleSpinningWait(true);
    this.DisableForSubmit(true);
}