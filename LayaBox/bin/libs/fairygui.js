;window.fairygui = window.fgui = {};
(function (fgui) {
    class AssetProxy {
        constructor() {
            this.loader = Laya.loader;
        }
        static get inst() {
            if (!AssetProxy._inst)
                AssetProxy._inst = new AssetProxy();
            return AssetProxy._inst;
        }
        getRes(url, type) {
            return this.loader.getRes(url, type);
        }
        getItemRes(item) {
            return this.getRes(item.file);
        }
        load(url, type, onProgress) {
            return this.loader.load(url, type, onProgress);
        }
    }
    fgui.AssetProxy = AssetProxy;
})(fgui);

(function (fgui) {
    class AsyncOperation {
        constructor() {
            this._itemList = new Array();
            this._objectPool = [];
        }
        createObject(pkgName, resName) {
            var pkg = fgui.UIPackage.getByName(pkgName);
            if (pkg) {
                var pi = pkg.getItemByName(resName);
                if (!pi)
                    throw new Error("resource not found: " + resName);
                this.internalCreateObject(pi);
            }
            else
                throw new Error("package not found: " + pkgName);
        }
        createObjectFromURL(url) {
            var pi = fgui.UIPackage.getItemByURL(url);
            if (pi)
                this.internalCreateObject(pi);
            else
                throw new Error("resource not found: " + url);
        }
        cancel() {
            Laya.timer.clear(this, this.run);
            this._itemList.length = 0;
            if (this._objectPool.length > 0) {
                var cnt = this._objectPool.length;
                for (var i = 0; i < cnt; i++) {
                    this._objectPool[i].dispose();
                }
                this._objectPool.length = 0;
            }
        }
        internalCreateObject(item) {
            this._itemList.length = 0;
            this._objectPool.length = 0;
            var di = { pi: item, type: item.objectType };
            di.childCount = this.collectComponentChildren(item);
            this._itemList.push(di);
            this._index = 0;
            Laya.timer.frameLoop(1, this, this.run);
        }
        collectComponentChildren(item) {
            var buffer = item.rawData;
            buffer.seek(0, 2);
            var di;
            var pi;
            var i;
            var dataLen;
            var curPos;
            var pkg;
            var dcnt = buffer.getInt16();
            for (i = 0; i < dcnt; i++) {
                dataLen = buffer.getInt16();
                curPos = buffer.pos;
                buffer.seek(curPos, 0);
                var type = buffer.readByte();
                var src = buffer.readS();
                var pkgId = buffer.readS();
                buffer.pos = curPos;
                if (src != null) {
                    if (pkgId != null)
                        pkg = fgui.UIPackage.getById(pkgId);
                    else
                        pkg = item.owner;
                    pi = pkg ? pkg.getItemById(src) : null;
                    di = { pi: pi, type: type };
                    if (pi && pi.type == fgui.PackageItemType.Component)
                        di.childCount = this.collectComponentChildren(pi);
                }
                else {
                    di = { type: type };
                    if (type == fgui.ObjectType.List) //list
                        di.listItemCount = this.collectListChildren(buffer);
                }
                this._itemList.push(di);
                buffer.pos = curPos + dataLen;
            }
            return dcnt;
        }
        collectListChildren(buffer) {
            buffer.seek(buffer.pos, 8);
            var listItemCount = 0;
            var i;
            var nextPos;
            var url;
            var pi;
            var di;
            var defaultItem = buffer.readS();
            var itemCount = buffer.getInt16();
            for (i = 0; i < itemCount; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                url = buffer.readS();
                if (url == null)
                    url = defaultItem;
                if (url) {
                    pi = fgui.UIPackage.getItemByURL(url);
                    if (pi) {
                        di = { pi: pi, type: pi.objectType };
                        if (pi.type == fgui.PackageItemType.Component)
                            di.childCount = this.collectComponentChildren(pi);
                        this._itemList.push(di);
                        listItemCount++;
                    }
                }
                buffer.pos = nextPos;
            }
            return listItemCount;
        }
        run() {
            var obj;
            var di;
            var poolStart;
            var k;
            var t = Laya.Browser.now();
            var frameTime = fgui.UIConfig.frameTimeForAsyncUIConstruction;
            var totalItems = this._itemList.length;
            while (this._index < totalItems) {
                di = this._itemList[this._index];
                if (di.pi) {
                    obj = fgui.UIObjectFactory.newObject(di.pi);
                    this._objectPool.push(obj);
                    fgui.UIPackage._constructing++;
                    if (di.pi.type == fgui.PackageItemType.Component) {
                        poolStart = this._objectPool.length - di.childCount - 1;
                        obj.constructFromResource2(this._objectPool, poolStart);
                        this._objectPool.splice(poolStart, di.childCount);
                    }
                    else {
                        obj.constructFromResource();
                    }
                    fgui.UIPackage._constructing--;
                }
                else {
                    obj = fgui.UIObjectFactory.newObject(di.type);
                    this._objectPool.push(obj);
                    if (di.type == fgui.ObjectType.List && di.listItemCount > 0) {
                        poolStart = this._objectPool.length - di.listItemCount - 1;
                        for (k = 0; k < di.listItemCount; k++) //把他们都放到pool里，这样GList在创建时就不需要创建对象了
                            obj.itemPool.returnObject(this._objectPool[k + poolStart]);
                        this._objectPool.splice(poolStart, di.listItemCount);
                    }
                }
                this._index++;
                if ((this._index % 5 == 0) && Laya.Browser.now() - t >= frameTime)
                    return;
            }
            Laya.timer.clear(this, this.run);
            var result = this._objectPool[0];
            this._itemList.length = 0;
            this._objectPool.length = 0;
            if (typeof this.callback === 'function')
                this.callback(result);
            else if (this.callback)
                this.callback.runWith(result);
        }
    }
    fgui.AsyncOperation = AsyncOperation;
})(fgui);

(function (fgui) {
    var _nextPageId = 0;
    class Controller extends Laya.EventDispatcher {
        constructor() {
            super();
            this._pageIds = [];
            this._pageNames = [];
            this._selectedIndex = -1;
            this._previousIndex = -1;
        }
        dispose() {
            this.offAll();
        }
        get selectedIndex() {
            return this._selectedIndex;
        }
        set selectedIndex(value) {
            if (this._selectedIndex != value) {
                if (value > this._pageIds.length - 1)
                    throw "index out of bounds: " + value;
                this.changing = true;
                this._previousIndex = this._selectedIndex;
                this._selectedIndex = value;
                this.parent.applyController(this);
                this.event(fgui.Events.STATE_CHANGED, this);
                this.changing = false;
            }
        }
        /**
         * 功能和设置selectedIndex一样，但不会触发事件
         */
        setSelectedIndex(value) {
            if (this._selectedIndex != value) {
                if (value > this._pageIds.length - 1)
                    throw "index out of bounds: " + value;
                this.changing = true;
                this._previousIndex = this._selectedIndex;
                this._selectedIndex = value;
                this.parent.applyController(this);
                this.changing = false;
            }
        }
        get previsousIndex() {
            return this._previousIndex;
        }
        get selectedPage() {
            if (this._selectedIndex == -1)
                return null;
            else
                return this._pageNames[this._selectedIndex];
        }
        set selectedPage(val) {
            var i = this._pageNames.indexOf(val);
            if (i == -1)
                i = 0;
            this.selectedIndex = i;
        }
        /**
         * 功能和设置selectedPage一样，但不会触发事件
         */
        setSelectedPage(value) {
            var i = this._pageNames.indexOf(value);
            if (i == -1)
                i = 0;
            this.setSelectedIndex(i);
        }
        get previousPage() {
            if (this._previousIndex == -1)
                return null;
            else
                return this._pageNames[this._previousIndex];
        }
        get pageCount() {
            return this._pageIds.length;
        }
        getPageName(index) {
            return this._pageNames[index];
        }
        addPage(name) {
            this.addPageAt(name, this._pageIds.length);
        }
        addPageAt(name, index) {
            var nid = "" + (_nextPageId++);
            if (index == this._pageIds.length) {
                this._pageIds.push(nid);
                this._pageNames.push(name);
            }
            else {
                this._pageIds.splice(index, 0, nid);
                this._pageNames.splice(index, 0, name);
            }
        }
        removePage(name) {
            var i = this._pageNames.indexOf(name);
            if (i != -1) {
                this._pageIds.splice(i, 1);
                this._pageNames.splice(i, 1);
                if (this._selectedIndex >= this._pageIds.length)
                    this.selectedIndex = this._selectedIndex - 1;
                else
                    this.parent.applyController(this);
            }
        }
        removePageAt(index) {
            this._pageIds.splice(index, 1);
            this._pageNames.splice(index, 1);
            if (this._selectedIndex >= this._pageIds.length)
                this.selectedIndex = this._selectedIndex - 1;
            else
                this.parent.applyController(this);
        }
        clearPages() {
            this._pageIds.length = 0;
            this._pageNames.length = 0;
            if (this._selectedIndex != -1)
                this.selectedIndex = -1;
            else
                this.parent.applyController(this);
        }
        hasPage(aName) {
            return this._pageNames.indexOf(aName) != -1;
        }
        getPageIndexById(aId) {
            return this._pageIds.indexOf(aId);
        }
        getPageIdByName(aName) {
            var i = this._pageNames.indexOf(aName);
            if (i != -1)
                return this._pageIds[i];
            else
                return null;
        }
        getPageNameById(aId) {
            var i = this._pageIds.indexOf(aId);
            if (i != -1)
                return this._pageNames[i];
            else
                return null;
        }
        getPageId(index) {
            return this._pageIds[index];
        }
        get selectedPageId() {
            if (this._selectedIndex == -1)
                return null;
            else
                return this._pageIds[this._selectedIndex];
        }
        set selectedPageId(val) {
            var i = this._pageIds.indexOf(val);
            this.selectedIndex = i;
        }
        set oppositePageId(val) {
            var i = this._pageIds.indexOf(val);
            if (i > 0)
                this.selectedIndex = 0;
            else if (this._pageIds.length > 1)
                this.selectedIndex = 1;
        }
        get previousPageId() {
            if (this._previousIndex == -1)
                return null;
            else
                return this._pageIds[this._previousIndex];
        }
        runActions() {
            if (this._actions) {
                var cnt = this._actions.length;
                for (var i = 0; i < cnt; i++) {
                    this._actions[i].run(this, this.previousPageId, this.selectedPageId);
                }
            }
        }
        setup(buffer) {
            var beginPos = buffer.pos;
            buffer.seek(beginPos, 0);
            this.name = buffer.readS();
            if (buffer.readBool())
                this.autoRadioGroupDepth = true;
            buffer.seek(beginPos, 1);
            var i;
            var nextPos;
            var cnt = buffer.getInt16();
            for (i = 0; i < cnt; i++) {
                this._pageIds.push(buffer.readS());
                this._pageNames.push(buffer.readS());
            }
            var homePageIndex = 0;
            if (buffer.version >= 2) {
                var homePageType = buffer.getByte();
                switch (homePageType) {
                    case 1:
                        homePageIndex = buffer.getInt16();
                        break;
                    case 2:
                        homePageIndex = this._pageNames.indexOf(fgui.UIPackage.branch);
                        if (homePageIndex == -1)
                            homePageIndex = 0;
                        break;
                    case 3:
                        homePageIndex = this._pageNames.indexOf(fgui.UIPackage.getVar(buffer.readS()));
                        if (homePageIndex == -1)
                            homePageIndex = 0;
                        break;
                }
            }
            buffer.seek(beginPos, 2);
            cnt = buffer.getInt16();
            if (cnt > 0) {
                if (!this._actions)
                    this._actions = [];
                for (i = 0; i < cnt; i++) {
                    nextPos = buffer.getInt16();
                    nextPos += buffer.pos;
                    var action = fgui.ControllerAction.createAction(buffer.readByte());
                    action.setup(buffer);
                    this._actions.push(action);
                    buffer.pos = nextPos;
                }
            }
            if (this.parent && this._pageIds.length > 0)
                this._selectedIndex = homePageIndex;
            else
                this._selectedIndex = -1;
        }
    }
    fgui.Controller = Controller;
})(fgui);

(function (fgui) {
    class DragDropManager {
        static get inst() {
            if (!DragDropManager._inst)
                DragDropManager._inst = new DragDropManager();
            return DragDropManager._inst;
        }
        constructor() {
            this._agent = new fgui.GLoader();
            this._agent.draggable = true;
            this._agent.touchable = false; ////important
            this._agent.setSize(100, 100);
            this._agent.setPivot(0.5, 0.5, true);
            this._agent.align = "center";
            this._agent.verticalAlign = "middle";
            this._agent.sortingOrder = 1000000;
            this._agent.on(fgui.Events.DRAG_END, this, this.__dragEnd);
        }
        get dragAgent() {
            return this._agent;
        }
        get dragging() {
            return this._agent.parent != null;
        }
        startDrag(source, icon, sourceData, touchID) {
            if (this._agent.parent)
                return;
            this._sourceData = sourceData;
            this._agent.url = icon;
            fgui.GRoot.inst.addChild(this._agent);
            var pt = fgui.GRoot.inst.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY);
            this._agent.setXY(pt.x, pt.y);
            this._agent.startDrag(touchID);
        }
        cancel() {
            if (this._agent.parent) {
                this._agent.stopDrag();
                fgui.GRoot.inst.removeChild(this._agent);
                this._sourceData = null;
            }
        }
        __dragEnd(evt) {
            if (!this._agent.parent) //cancelled
                return;
            fgui.GRoot.inst.removeChild(this._agent);
            var sourceData = this._sourceData;
            this._sourceData = null;
            var obj = fgui.GObject.cast(evt.target);
            while (obj) {
                if (obj.displayObject.hasListener(fgui.Events.DROP)) {
                    obj.requestFocus();
                    obj.displayObject.event(fgui.Events.DROP, [sourceData, fgui.Events.createEvent(fgui.Events.DROP, obj.displayObject, evt)]);
                    return;
                }
                obj = obj.parent;
            }
        }
    }
    fgui.DragDropManager = DragDropManager;
})(fgui);

(function (fgui) {
    class Events {
        static createEvent(type, target, source) {
            this.$event.setTo(type, target, source ? (source.target || target) : target);
            this.$event.touchId = source ? (source.touchId || 0) : 0;
            this.$event.nativeEvent = source;
            this.$event["_stoped"] = false;
            return this.$event;
        }
        static dispatch(type, target, source) {
            target.event(type, this.createEvent(type, target, source));
        }
    }
    Events.BLUR = "fui_blur";
    Events.FOCUS = "fui_focus";
    Events.STATE_CHANGED = "fui_state_changed";
    Events.XY_CHANGED = "fui_xy_changed";
    Events.SIZE_CHANGED = "fui_size_changed";
    Events.SIZE_DELAY_CHANGE = "fui_size_delay_change";
    Events.CLICK_ITEM = "fui_click_item";
    Events.SCROLL = "fui_scroll";
    Events.SCROLL_END = "fui_scroll_end";
    Events.DROP = "fui_drop";
    Events.DRAG_START = "fui_drag_start";
    Events.DRAG_MOVE = "fui_drag_move";
    Events.DRAG_END = "fui_drag_end";
    Events.PULL_DOWN_RELEASE = "fui_pull_down_release";
    Events.PULL_UP_RELEASE = "fui_pull_up_release";
    Events.GEAR_STOP = "fui_gear_stop";
    Events.$event = new Laya.Event();
    fgui.Events = Events;
})(fgui);

(function (fgui) {
    let ButtonMode;
    (function (ButtonMode) {
        ButtonMode[ButtonMode["Common"] = 0] = "Common";
        ButtonMode[ButtonMode["Check"] = 1] = "Check";
        ButtonMode[ButtonMode["Radio"] = 2] = "Radio";
    })(ButtonMode = fgui.ButtonMode || (fgui.ButtonMode = {}));
    let AutoSizeType;
    (function (AutoSizeType) {
        AutoSizeType[AutoSizeType["None"] = 0] = "None";
        AutoSizeType[AutoSizeType["Both"] = 1] = "Both";
        AutoSizeType[AutoSizeType["Height"] = 2] = "Height";
        AutoSizeType[AutoSizeType["Shrink"] = 3] = "Shrink";
        AutoSizeType[AutoSizeType["Ellipsis"] = 4] = "Ellipsis";
    })(AutoSizeType = fgui.AutoSizeType || (fgui.AutoSizeType = {}));
    let AlignType;
    (function (AlignType) {
        AlignType[AlignType["Left"] = 0] = "Left";
        AlignType[AlignType["Center"] = 1] = "Center";
        AlignType[AlignType["Right"] = 2] = "Right";
    })(AlignType = fgui.AlignType || (fgui.AlignType = {}));
    let VertAlignType;
    (function (VertAlignType) {
        VertAlignType[VertAlignType["Top"] = 0] = "Top";
        VertAlignType[VertAlignType["Middle"] = 1] = "Middle";
        VertAlignType[VertAlignType["Bottom"] = 2] = "Bottom";
    })(VertAlignType = fgui.VertAlignType || (fgui.VertAlignType = {}));
    let LoaderFillType;
    (function (LoaderFillType) {
        LoaderFillType[LoaderFillType["None"] = 0] = "None";
        LoaderFillType[LoaderFillType["Scale"] = 1] = "Scale";
        LoaderFillType[LoaderFillType["ScaleMatchHeight"] = 2] = "ScaleMatchHeight";
        LoaderFillType[LoaderFillType["ScaleMatchWidth"] = 3] = "ScaleMatchWidth";
        LoaderFillType[LoaderFillType["ScaleFree"] = 4] = "ScaleFree";
        LoaderFillType[LoaderFillType["ScaleNoBorder"] = 5] = "ScaleNoBorder";
        /**
         * 组件根据 GLoader 的大小自动调整尺寸的适配模式
         * @note 2023/03/03 编辑器中还没有这种适配模式，暂时用 ScaleFree 代替
         */
        LoaderFillType[LoaderFillType["Resize"] = 4] = "Resize";
    })(LoaderFillType = fgui.LoaderFillType || (fgui.LoaderFillType = {}));
    let ListLayoutType;
    (function (ListLayoutType) {
        ListLayoutType[ListLayoutType["SingleColumn"] = 0] = "SingleColumn";
        ListLayoutType[ListLayoutType["SingleRow"] = 1] = "SingleRow";
        ListLayoutType[ListLayoutType["FlowHorizontal"] = 2] = "FlowHorizontal";
        ListLayoutType[ListLayoutType["FlowVertical"] = 3] = "FlowVertical";
        ListLayoutType[ListLayoutType["Pagination"] = 4] = "Pagination";
    })(ListLayoutType = fgui.ListLayoutType || (fgui.ListLayoutType = {}));
    let ListSelectionMode;
    (function (ListSelectionMode) {
        ListSelectionMode[ListSelectionMode["Single"] = 0] = "Single";
        ListSelectionMode[ListSelectionMode["Multiple"] = 1] = "Multiple";
        ListSelectionMode[ListSelectionMode["Multiple_SingleClick"] = 2] = "Multiple_SingleClick";
        ListSelectionMode[ListSelectionMode["None"] = 3] = "None";
    })(ListSelectionMode = fgui.ListSelectionMode || (fgui.ListSelectionMode = {}));
    let OverflowType;
    (function (OverflowType) {
        OverflowType[OverflowType["Visible"] = 0] = "Visible";
        OverflowType[OverflowType["Hidden"] = 1] = "Hidden";
        OverflowType[OverflowType["Scroll"] = 2] = "Scroll";
    })(OverflowType = fgui.OverflowType || (fgui.OverflowType = {}));
    let PackageItemType;
    (function (PackageItemType) {
        PackageItemType[PackageItemType["Image"] = 0] = "Image";
        PackageItemType[PackageItemType["MovieClip"] = 1] = "MovieClip";
        PackageItemType[PackageItemType["Sound"] = 2] = "Sound";
        PackageItemType[PackageItemType["Component"] = 3] = "Component";
        PackageItemType[PackageItemType["Atlas"] = 4] = "Atlas";
        PackageItemType[PackageItemType["Font"] = 5] = "Font";
        PackageItemType[PackageItemType["Swf"] = 6] = "Swf";
        PackageItemType[PackageItemType["Misc"] = 7] = "Misc";
        PackageItemType[PackageItemType["Unknown"] = 8] = "Unknown";
        PackageItemType[PackageItemType["Spine"] = 9] = "Spine";
        PackageItemType[PackageItemType["DragonBones"] = 10] = "DragonBones";
    })(PackageItemType = fgui.PackageItemType || (fgui.PackageItemType = {}));
    let ObjectType;
    (function (ObjectType) {
        ObjectType[ObjectType["Image"] = 0] = "Image";
        ObjectType[ObjectType["MovieClip"] = 1] = "MovieClip";
        ObjectType[ObjectType["Swf"] = 2] = "Swf";
        ObjectType[ObjectType["Graph"] = 3] = "Graph";
        ObjectType[ObjectType["Loader"] = 4] = "Loader";
        ObjectType[ObjectType["Group"] = 5] = "Group";
        ObjectType[ObjectType["Text"] = 6] = "Text";
        ObjectType[ObjectType["RichText"] = 7] = "RichText";
        ObjectType[ObjectType["InputText"] = 8] = "InputText";
        ObjectType[ObjectType["Component"] = 9] = "Component";
        ObjectType[ObjectType["List"] = 10] = "List";
        ObjectType[ObjectType["Label"] = 11] = "Label";
        ObjectType[ObjectType["Button"] = 12] = "Button";
        ObjectType[ObjectType["ComboBox"] = 13] = "ComboBox";
        ObjectType[ObjectType["ProgressBar"] = 14] = "ProgressBar";
        ObjectType[ObjectType["Slider"] = 15] = "Slider";
        ObjectType[ObjectType["ScrollBar"] = 16] = "ScrollBar";
        ObjectType[ObjectType["Tree"] = 17] = "Tree";
        ObjectType[ObjectType["Loader3D"] = 18] = "Loader3D";
    })(ObjectType = fgui.ObjectType || (fgui.ObjectType = {}));
    let ProgressTitleType;
    (function (ProgressTitleType) {
        ProgressTitleType[ProgressTitleType["Percent"] = 0] = "Percent";
        ProgressTitleType[ProgressTitleType["ValueAndMax"] = 1] = "ValueAndMax";
        ProgressTitleType[ProgressTitleType["Value"] = 2] = "Value";
        ProgressTitleType[ProgressTitleType["Max"] = 3] = "Max";
    })(ProgressTitleType = fgui.ProgressTitleType || (fgui.ProgressTitleType = {}));
    let ScrollBarDisplayType;
    (function (ScrollBarDisplayType) {
        ScrollBarDisplayType[ScrollBarDisplayType["Default"] = 0] = "Default";
        ScrollBarDisplayType[ScrollBarDisplayType["Visible"] = 1] = "Visible";
        ScrollBarDisplayType[ScrollBarDisplayType["Auto"] = 2] = "Auto";
        ScrollBarDisplayType[ScrollBarDisplayType["Hidden"] = 3] = "Hidden";
    })(ScrollBarDisplayType = fgui.ScrollBarDisplayType || (fgui.ScrollBarDisplayType = {}));
    let ScrollType;
    (function (ScrollType) {
        ScrollType[ScrollType["Horizontal"] = 0] = "Horizontal";
        ScrollType[ScrollType["Vertical"] = 1] = "Vertical";
        ScrollType[ScrollType["Both"] = 2] = "Both";
    })(ScrollType = fgui.ScrollType || (fgui.ScrollType = {}));
    let FlipType;
    (function (FlipType) {
        FlipType[FlipType["None"] = 0] = "None";
        FlipType[FlipType["Horizontal"] = 1] = "Horizontal";
        FlipType[FlipType["Vertical"] = 2] = "Vertical";
        FlipType[FlipType["Both"] = 3] = "Both";
    })(FlipType = fgui.FlipType || (fgui.FlipType = {}));
    let ChildrenRenderOrder;
    (function (ChildrenRenderOrder) {
        ChildrenRenderOrder[ChildrenRenderOrder["Ascent"] = 0] = "Ascent";
        ChildrenRenderOrder[ChildrenRenderOrder["Descent"] = 1] = "Descent";
        ChildrenRenderOrder[ChildrenRenderOrder["Arch"] = 2] = "Arch";
    })(ChildrenRenderOrder = fgui.ChildrenRenderOrder || (fgui.ChildrenRenderOrder = {}));
    let GroupLayoutType;
    (function (GroupLayoutType) {
        GroupLayoutType[GroupLayoutType["None"] = 0] = "None";
        GroupLayoutType[GroupLayoutType["Horizontal"] = 1] = "Horizontal";
        GroupLayoutType[GroupLayoutType["Vertical"] = 2] = "Vertical";
    })(GroupLayoutType = fgui.GroupLayoutType || (fgui.GroupLayoutType = {}));
    let PopupDirection;
    (function (PopupDirection) {
        PopupDirection[PopupDirection["Auto"] = 0] = "Auto";
        PopupDirection[PopupDirection["Up"] = 1] = "Up";
        PopupDirection[PopupDirection["Down"] = 2] = "Down";
    })(PopupDirection = fgui.PopupDirection || (fgui.PopupDirection = {}));
    let RelationType;
    (function (RelationType) {
        RelationType[RelationType["Left_Left"] = 0] = "Left_Left";
        RelationType[RelationType["Left_Center"] = 1] = "Left_Center";
        RelationType[RelationType["Left_Right"] = 2] = "Left_Right";
        RelationType[RelationType["Center_Center"] = 3] = "Center_Center";
        RelationType[RelationType["Right_Left"] = 4] = "Right_Left";
        RelationType[RelationType["Right_Center"] = 5] = "Right_Center";
        RelationType[RelationType["Right_Right"] = 6] = "Right_Right";
        RelationType[RelationType["Top_Top"] = 7] = "Top_Top";
        RelationType[RelationType["Top_Middle"] = 8] = "Top_Middle";
        RelationType[RelationType["Top_Bottom"] = 9] = "Top_Bottom";
        RelationType[RelationType["Middle_Middle"] = 10] = "Middle_Middle";
        RelationType[RelationType["Bottom_Top"] = 11] = "Bottom_Top";
        RelationType[RelationType["Bottom_Middle"] = 12] = "Bottom_Middle";
        RelationType[RelationType["Bottom_Bottom"] = 13] = "Bottom_Bottom";
        RelationType[RelationType["Width"] = 14] = "Width";
        RelationType[RelationType["Height"] = 15] = "Height";
        RelationType[RelationType["LeftExt_Left"] = 16] = "LeftExt_Left";
        RelationType[RelationType["LeftExt_Right"] = 17] = "LeftExt_Right";
        RelationType[RelationType["RightExt_Left"] = 18] = "RightExt_Left";
        RelationType[RelationType["RightExt_Right"] = 19] = "RightExt_Right";
        RelationType[RelationType["TopExt_Top"] = 20] = "TopExt_Top";
        RelationType[RelationType["TopExt_Bottom"] = 21] = "TopExt_Bottom";
        RelationType[RelationType["BottomExt_Top"] = 22] = "BottomExt_Top";
        RelationType[RelationType["BottomExt_Bottom"] = 23] = "BottomExt_Bottom";
        RelationType[RelationType["Size"] = 24] = "Size";
    })(RelationType = fgui.RelationType || (fgui.RelationType = {}));
    let FillMethod;
    (function (FillMethod) {
        FillMethod[FillMethod["None"] = 0] = "None";
        FillMethod[FillMethod["Horizontal"] = 1] = "Horizontal";
        FillMethod[FillMethod["Vertical"] = 2] = "Vertical";
        FillMethod[FillMethod["Radial90"] = 3] = "Radial90";
        FillMethod[FillMethod["Radial180"] = 4] = "Radial180";
        FillMethod[FillMethod["Radial360"] = 5] = "Radial360";
    })(FillMethod = fgui.FillMethod || (fgui.FillMethod = {}));
    let FillOrigin;
    (function (FillOrigin) {
        FillOrigin[FillOrigin["Top"] = 0] = "Top";
        FillOrigin[FillOrigin["Bottom"] = 1] = "Bottom";
        FillOrigin[FillOrigin["Left"] = 2] = "Left";
        FillOrigin[FillOrigin["Right"] = 3] = "Right";
        FillOrigin[FillOrigin["TopLeft"] = 0] = "TopLeft";
        FillOrigin[FillOrigin["TopRight"] = 1] = "TopRight";
        FillOrigin[FillOrigin["BottomLeft"] = 2] = "BottomLeft";
        FillOrigin[FillOrigin["BottomRight"] = 3] = "BottomRight";
    })(FillOrigin = fgui.FillOrigin || (fgui.FillOrigin = {}));
    let FillOrigin90;
    (function (FillOrigin90) {
        FillOrigin90[FillOrigin90["TopLeft"] = 0] = "TopLeft";
        FillOrigin90[FillOrigin90["TopRight"] = 1] = "TopRight";
        FillOrigin90[FillOrigin90["BottomLeft"] = 2] = "BottomLeft";
        FillOrigin90[FillOrigin90["BottomRight"] = 3] = "BottomRight";
    })(FillOrigin90 = fgui.FillOrigin90 || (fgui.FillOrigin90 = {}));
    let ObjectPropID;
    (function (ObjectPropID) {
        ObjectPropID[ObjectPropID["Text"] = 0] = "Text";
        ObjectPropID[ObjectPropID["Icon"] = 1] = "Icon";
        ObjectPropID[ObjectPropID["Color"] = 2] = "Color";
        ObjectPropID[ObjectPropID["OutlineColor"] = 3] = "OutlineColor";
        ObjectPropID[ObjectPropID["Playing"] = 4] = "Playing";
        ObjectPropID[ObjectPropID["Frame"] = 5] = "Frame";
        ObjectPropID[ObjectPropID["DeltaTime"] = 6] = "DeltaTime";
        ObjectPropID[ObjectPropID["TimeScale"] = 7] = "TimeScale";
        ObjectPropID[ObjectPropID["FontSize"] = 8] = "FontSize";
        ObjectPropID[ObjectPropID["Selected"] = 9] = "Selected";
    })(ObjectPropID = fgui.ObjectPropID || (fgui.ObjectPropID = {}));
})(fgui);

(function (fgui) {
    class GObject {
        constructor() {
            this._x = 0;
            this._y = 0;
            this._alpha = 1;
            this._rotation = 0;
            this._visible = true;
            this._touchable = true;
            this._scaleX = 1;
            this._scaleY = 1;
            this._skewX = 0;
            this._skewY = 0;
            this._pivotX = 0;
            this._pivotY = 0;
            this._pivotOffsetX = 0;
            this._pivotOffsetY = 0;
            this._sortingOrder = 0;
            this._internalVisible = true;
            this._yOffset = 0;
            this.minWidth = 0;
            this.minHeight = 0;
            this.maxWidth = 0;
            this.maxHeight = 0;
            this.sourceWidth = 0;
            this.sourceHeight = 0;
            this.initWidth = 0;
            this.initHeight = 0;
            this._width = 0;
            this._height = 0;
            this._rawWidth = 0;
            this._rawHeight = 0;
            this._sizePercentInGroup = 0;
            this._id = "" + _gInstanceCounter++;
            this._name = "";
            this.createDisplayObject();
            this._relations = new fgui.Relations(this);
            this._gears = new Array(10);
        }
        get id() {
            return this._id;
        }
        get name() {
            return this._name;
        }
        set name(value) {
            this._name = value;
        }
        get x() {
            return this._x;
        }
        set x(value) {
            this.setXY(value, this._y);
        }
        get y() {
            return this._y;
        }
        set y(value) {
            this.setXY(this._x, value);
        }
        setXY(xv, yv) {
            if (this._x != xv || this._y != yv) {
                var dx = xv - this._x;
                var dy = yv - this._y;
                this._x = xv;
                this._y = yv;
                this.handleXYChanged();
                if (this instanceof fgui.GGroup)
                    this.moveChildren(dx, dy);
                this.updateGear(1);
                if (this._parent && !(this._parent instanceof fgui.GList)) {
                    this._parent.setBoundsChangedFlag();
                    if (this._group)
                        this._group.setBoundsChangedFlag(true);
                    this.displayObject.event(fgui.Events.XY_CHANGED);
                }
                if (GObject.draggingObject == this && !sUpdateInDragging)
                    this.localToGlobalRect(0, 0, this.width, this.height, sGlobalRect);
            }
        }
        get xMin() {
            return this._pivotAsAnchor ? (this._x - this._width * this._pivotX) : this._x;
        }
        set xMin(value) {
            if (this._pivotAsAnchor)
                this.setXY(value + this._width * this._pivotX, this._y);
            else
                this.setXY(value, this._y);
        }
        get yMin() {
            return this._pivotAsAnchor ? (this._y - this._height * this._pivotY) : this._y;
        }
        set yMin(value) {
            if (this._pivotAsAnchor)
                this.setXY(this._x, value + this._height * this._pivotY);
            else
                this.setXY(this._x, value);
        }
        get pixelSnapping() {
            return this._pixelSnapping;
        }
        set pixelSnapping(value) {
            if (this._pixelSnapping != value) {
                this._pixelSnapping = value;
                this.handleXYChanged();
            }
        }
        center(restraint) {
            var r;
            if (this._parent)
                r = this.parent;
            else
                r = this.root;
            this.setXY((r.width - this.width) / 2, (r.height - this.height) / 2);
            if (restraint) {
                this.addRelation(r, fgui.RelationType.Center_Center);
                this.addRelation(r, fgui.RelationType.Middle_Middle);
            }
        }
        get width() {
            this.ensureSizeCorrect();
            if (this._relations.sizeDirty)
                this._relations.ensureRelationsSizeCorrect();
            return this._width;
        }
        set width(value) {
            this.setSize(value, this._rawHeight);
        }
        get height() {
            this.ensureSizeCorrect();
            if (this._relations.sizeDirty)
                this._relations.ensureRelationsSizeCorrect();
            return this._height;
        }
        set height(value) {
            this.setSize(this._rawWidth, value);
        }
        setSize(wv, hv, ignorePivot) {
            if (this._rawWidth != wv || this._rawHeight != hv) {
                this._rawWidth = wv;
                this._rawHeight = hv;
                if (wv < this.minWidth)
                    wv = this.minWidth;
                if (hv < this.minHeight)
                    hv = this.minHeight;
                if (this.maxWidth > 0 && wv > this.maxWidth)
                    wv = this.maxWidth;
                if (this.maxHeight > 0 && hv > this.maxHeight)
                    hv = this.maxHeight;
                var dWidth = wv - this._width;
                var dHeight = hv - this._height;
                this._width = wv;
                this._height = hv;
                this.handleSizeChanged();
                if (this._pivotX != 0 || this._pivotY != 0) {
                    if (!this._pivotAsAnchor) {
                        if (!ignorePivot)
                            this.setXY(this.x - this._pivotX * dWidth, this.y - this._pivotY * dHeight);
                        this.updatePivotOffset();
                    }
                    else
                        this.applyPivot();
                }
                if (this instanceof fgui.GGroup)
                    this.resizeChildren(dWidth, dHeight);
                this.updateGear(2);
                if (this._parent) {
                    this._relations.onOwnerSizeChanged(dWidth, dHeight, this._pivotAsAnchor || !ignorePivot);
                    this._parent.setBoundsChangedFlag();
                    if (this._group)
                        this._group.setBoundsChangedFlag();
                }
                this.displayObject.event(fgui.Events.SIZE_CHANGED);
            }
        }
        ensureSizeCorrect() {
        }
        makeFullScreen() {
            this.setSize(fgui.GRoot.inst.width, fgui.GRoot.inst.height);
        }
        get actualWidth() {
            return this.width * Math.abs(this._scaleX);
        }
        get actualHeight() {
            return this.height * Math.abs(this._scaleY);
        }
        get scaleX() {
            return this._scaleX;
        }
        set scaleX(value) {
            this.setScale(value, this._scaleY);
        }
        get scaleY() {
            return this._scaleY;
        }
        set scaleY(value) {
            this.setScale(this._scaleX, value);
        }
        setScale(sx, sy) {
            if (this._scaleX != sx || this._scaleY != sy) {
                this._scaleX = sx;
                this._scaleY = sy;
                this.handleScaleChanged();
                this.applyPivot();
                this.updateGear(2);
            }
        }
        get skewX() {
            return this._skewX;
        }
        set skewX(value) {
            this.setSkew(value, this._skewY);
        }
        get skewY() {
            return this._skewY;
        }
        set skewY(value) {
            this.setSkew(this._skewX, value);
        }
        setSkew(sx, sy) {
            if (this._skewX != sx || this._skewY != sy) {
                this._skewX = sx;
                this._skewY = sy;
                if (this._displayObject) {
                    this._displayObject.skew(-sx, sy);
                    this.applyPivot();
                }
            }
        }
        get pivotX() {
            return this._pivotX;
        }
        set pivotX(value) {
            this.setPivot(value, this._pivotY);
        }
        get pivotY() {
            return this._pivotY;
        }
        set pivotY(value) {
            this.setPivot(this._pivotX, value);
        }
        setPivot(xv, yv = 0, asAnchor) {
            if (this._pivotX != xv || this._pivotY != yv || this._pivotAsAnchor != asAnchor) {
                this._pivotX = xv;
                this._pivotY = yv;
                this._pivotAsAnchor = asAnchor;
                this.updatePivotOffset();
                this.handleXYChanged();
            }
        }
        get pivotAsAnchor() {
            return this._pivotAsAnchor;
        }
        internalSetPivot(xv, yv, asAnchor) {
            this._pivotX = xv;
            this._pivotY = yv;
            this._pivotAsAnchor = asAnchor;
            if (this._pivotAsAnchor)
                this.handleXYChanged();
        }
        updatePivotOffset() {
            if (this._displayObject) {
                if (this._displayObject.transform && (this._pivotX != 0 || this._pivotY != 0)) {
                    sHelperPoint.x = this._pivotX * this._width;
                    sHelperPoint.y = this._pivotY * this._height;
                    var pt = this._displayObject.transform.transformPoint(sHelperPoint);
                    this._pivotOffsetX = this._pivotX * this._width - pt.x;
                    this._pivotOffsetY = this._pivotY * this._height - pt.y;
                }
                else {
                    this._pivotOffsetX = 0;
                    this._pivotOffsetY = 0;
                }
            }
        }
        applyPivot() {
            if (this._pivotX != 0 || this._pivotY != 0) {
                this.updatePivotOffset();
                this.handleXYChanged();
            }
        }
        get touchable() {
            return this._touchable;
        }
        set touchable(value) {
            if (this._touchable != value) {
                this._touchable = value;
                this.updateGear(3);
                if ((this instanceof fgui.GImage) || (this instanceof fgui.GMovieClip)
                    || (this instanceof fgui.GTextField) && !(this instanceof fgui.GTextInput) && !(this instanceof fgui.GRichTextField))
                    //Touch is not supported by GImage/GMovieClip/GTextField
                    return;
                if (this._displayObject)
                    this._displayObject.mouseEnabled = this._touchable;
            }
        }
        get grayed() {
            return this._grayed;
        }
        set grayed(value) {
            if (this._grayed != value) {
                this._grayed = value;
                this.handleGrayedChanged();
                this.updateGear(3);
            }
        }
        get enabled() {
            return !this._grayed && this._touchable;
        }
        set enabled(value) {
            this.grayed = !value;
            this.touchable = value;
        }
        get rotation() {
            return this._rotation;
        }
        set rotation(value) {
            if (this._rotation != value) {
                this._rotation = value;
                if (this._displayObject) {
                    this._displayObject.rotation = this.normalizeRotation;
                    this.applyPivot();
                }
                this.updateGear(3);
            }
        }
        get normalizeRotation() {
            var rot = this._rotation % 360;
            if (rot > 180)
                rot = rot - 360;
            else if (rot < -180)
                rot = 360 + rot;
            return rot;
        }
        get alpha() {
            return this._alpha;
        }
        set alpha(value) {
            if (this._alpha != value) {
                this._alpha = value;
                this.handleAlphaChanged();
                this.updateGear(3);
            }
        }
        get visible() {
            return this._visible;
        }
        set visible(value) {
            if (this._visible != value) {
                this._visible = value;
                this.handleVisibleChanged();
                if (this._parent)
                    this._parent.setBoundsChangedFlag();
                if (this._group && this._group.excludeInvisibles)
                    this._group.setBoundsChangedFlag();
            }
        }
        get internalVisible() {
            return this._internalVisible && (!this._group || this._group.internalVisible)
                && !this._displayObject._cacheStyle.maskParent;
        }
        get internalVisible2() {
            return this._visible && (!this._group || this._group.internalVisible2);
        }
        get internalVisible3() {
            return this._internalVisible && this._visible;
        }
        get sortingOrder() {
            return this._sortingOrder;
        }
        set sortingOrder(value) {
            if (value < 0)
                value = 0;
            if (this._sortingOrder != value) {
                var old = this._sortingOrder;
                this._sortingOrder = value;
                if (this._parent)
                    this._parent.childSortingOrderChanged(this, old, this._sortingOrder);
            }
        }
        get focused() {
            return this.root.focus == this;
        }
        requestFocus() {
            this.root.focus = this;
        }
        get tooltips() {
            return this._tooltips;
        }
        set tooltips(value) {
            if (this._tooltips) {
                this.off(Laya.Event.ROLL_OVER, this, this.__rollOver);
                this.off(Laya.Event.ROLL_OUT, this, this.__rollOut);
            }
            this._tooltips = value;
            if (this._tooltips) {
                this.on(Laya.Event.ROLL_OVER, this, this.__rollOver);
                this.on(Laya.Event.ROLL_OUT, this, this.__rollOut);
            }
        }
        __rollOver(evt) {
            Laya.timer.once(100, this, this.__doShowTooltips);
        }
        __doShowTooltips() {
            var r = this.root;
            if (r)
                this.root.showTooltips(this._tooltips);
        }
        __rollOut(evt) {
            Laya.timer.clear(this, this.__doShowTooltips);
            this.root.hideTooltips();
        }
        get blendMode() {
            return this._displayObject.blendMode;
        }
        set blendMode(value) {
            this._displayObject.blendMode = value;
        }
        get filters() {
            return this._displayObject.filters;
        }
        set filters(value) {
            this._displayObject.filters = value;
        }
        get inContainer() {
            return this._displayObject != null && this._displayObject.parent != null;
        }
        get onStage() {
            return this._displayObject != null && this._displayObject.stage != null;
        }
        get resourceURL() {
            if (this.packageItem)
                return "ui://" + this.packageItem.owner.id + this.packageItem.id;
            else
                return null;
        }
        set group(value) {
            if (this._group != value) {
                if (this._group)
                    this._group.setBoundsChangedFlag();
                this._group = value;
                if (this._group)
                    this._group.setBoundsChangedFlag();
            }
        }
        get group() {
            return this._group;
        }
        getGear(index) {
            var gear = this._gears[index];
            if (!gear)
                this._gears[index] = gear = fgui.GearBase.create(this, index);
            return gear;
        }
        updateGear(index) {
            if (this._underConstruct || this._gearLocked)
                return;
            var gear = this._gears[index];
            if (gear && gear.controller)
                gear.updateState();
        }
        checkGearController(index, c) {
            return this._gears[index] && this._gears[index].controller == c;
        }
        updateGearFromRelations(index, dx, dy) {
            if (this._gears[index])
                this._gears[index].updateFromRelations(dx, dy);
        }
        addDisplayLock() {
            var gearDisplay = (this._gears[0]);
            if (gearDisplay && gearDisplay.controller) {
                var ret = gearDisplay.addLock();
                this.checkGearDisplay();
                return ret;
            }
            else
                return 0;
        }
        releaseDisplayLock(token) {
            var gearDisplay = (this._gears[0]);
            if (gearDisplay && gearDisplay.controller) {
                gearDisplay.releaseLock(token);
                this.checkGearDisplay();
            }
        }
        checkGearDisplay() {
            if (this._handlingController)
                return;
            var connected = !this._gears[0] || (this._gears[0]).connected;
            if (this._gears[8])
                connected = this._gears[8].evaluate(connected);
            if (connected != this._internalVisible) {
                this._internalVisible = connected;
                if (this._parent) {
                    this._parent.childStateChanged(this);
                    if (this._group && this._group.excludeInvisibles)
                        this._group.setBoundsChangedFlag();
                }
            }
        }
        get relations() {
            return this._relations;
        }
        addRelation(target, relationType, usePercent) {
            this._relations.add(target, relationType, usePercent);
        }
        removeRelation(target, relationType) {
            this._relations.remove(target, relationType);
        }
        get displayObject() {
            return this._displayObject;
        }
        get parent() {
            return this._parent;
        }
        set parent(val) {
            this._parent = val;
        }
        removeFromParent() {
            if (this._parent)
                this._parent.removeChild(this);
        }
        get root() {
            if (this instanceof fgui.GRoot)
                return this;
            var p = this._parent;
            while (p) {
                if (p instanceof fgui.GRoot)
                    return p;
                p = p.parent;
            }
            return fgui.GRoot.inst;
        }
        get asCom() {
            return this;
        }
        get asButton() {
            return this;
        }
        get asLabel() {
            return this;
        }
        get asProgress() {
            return this;
        }
        get asTextField() {
            return this;
        }
        get asRichTextField() {
            return this;
        }
        get asTextInput() {
            return this;
        }
        get asLoader() {
            return this;
        }
        get asList() {
            return this;
        }
        get asTree() {
            return this;
        }
        get asGraph() {
            return this;
        }
        get asGroup() {
            return this;
        }
        get asSlider() {
            return this;
        }
        get asComboBox() {
            return this;
        }
        get asImage() {
            return this;
        }
        get asMovieClip() {
            return this;
        }
        get text() {
            return null;
        }
        set text(value) {
        }
        get icon() {
            return null;
        }
        set icon(value) {
        }
        get treeNode() {
            return this._treeNode;
        }
        get isDisposed() {
            return this._displayObject == null;
        }
        dispose() {
            this.removeFromParent();
            this._relations.dispose();
            this._displayObject.destroy();
            this._displayObject = null;
            for (var i = 0; i < 10; i++) {
                var gear = this._gears[i];
                if (gear)
                    gear.dispose();
            }
        }
        onClick(thisObj, listener, args) {
            this.on(Laya.Event.CLICK, thisObj, listener, args);
        }
        offClick(thisObj, listener) {
            this.off(Laya.Event.CLICK, thisObj, listener);
        }
        hasClickListener() {
            return this._displayObject.hasListener(Laya.Event.CLICK);
        }
        on(type, thisObject, listener, args) {
            this._displayObject.on(type, thisObject, listener, args);
        }
        off(type, thisObject, listener) {
            this._displayObject.off(type, thisObject, listener);
        }
        get draggable() {
            return this._draggable;
        }
        set draggable(value) {
            if (this._draggable != value) {
                this._draggable = value;
                this.initDrag();
            }
        }
        get dragBounds() {
            return this._dragBounds;
        }
        set dragBounds(value) {
            this._dragBounds = value;
        }
        startDrag(touchID) {
            if (this._displayObject.stage == null)
                return;
            this.dragBegin(touchID);
        }
        stopDrag() {
            this.dragEnd();
        }
        get dragging() {
            return GObject.draggingObject == this;
        }
        localToGlobal(ax, ay, result) {
            ax = ax || 0;
            ay = ay || 0;
            if (this._pivotAsAnchor) {
                ax += this._pivotX * this._width;
                ay += this._pivotY * this._height;
            }
            result = result || new Laya.Point();
            result.x = ax;
            result.y = ay;
            return this._displayObject.localToGlobal(result, false);
        }
        globalToLocal(ax, ay, result) {
            ax = ax || 0;
            ay = ay || 0;
            result = result || new Laya.Point();
            result.x = ax;
            result.y = ay;
            result = this._displayObject.globalToLocal(result, false);
            if (this._pivotAsAnchor) {
                result.x -= this._pivotX * this._width;
                result.y -= this._pivotY * this._height;
            }
            return result;
        }
        localToGlobalRect(ax, ay, aw, ah, result) {
            ax = ax || 0;
            ay = ay || 0;
            aw = aw || 0;
            ah = ah || 0;
            result = result || new Laya.Rectangle();
            var pt = this.localToGlobal(ax, ay);
            result.x = pt.x;
            result.y = pt.y;
            pt = this.localToGlobal(ax + aw, ay + ah);
            result.width = pt.x - result.x;
            result.height = pt.y - result.y;
            return result;
        }
        globalToLocalRect(ax, ay, aw, ah, result) {
            ax = ax || 0;
            ay = ay || 0;
            aw = aw || 0;
            ah = ah || 0;
            result = result || new Laya.Rectangle();
            var pt = this.globalToLocal(ax, ay);
            result.x = pt.x;
            result.y = pt.y;
            pt = this.globalToLocal(ax + aw, ay + ah);
            result.width = pt.x - result.x;
            result.height = pt.y - result.y;
            return result;
        }
        handleControllerChanged(c) {
            this._handlingController = true;
            for (var i = 0; i < 10; i++) {
                var gear = this._gears[i];
                if (gear && gear.controller == c)
                    gear.apply();
            }
            this._handlingController = false;
            this.checkGearDisplay();
        }
        createDisplayObject() {
            this._displayObject = new Laya.Sprite();
            this._displayObject["$owner"] = this;
        }
        handleXYChanged() {
            var xv = this._x;
            var yv = this._y + this._yOffset;
            if (this._pivotAsAnchor) {
                xv -= this._pivotX * this._width;
                yv -= this._pivotY * this._height;
            }
            if (this._pixelSnapping) {
                xv = Math.round(xv);
                yv = Math.round(yv);
            }
            this._displayObject.pos(xv + this._pivotOffsetX, yv + this._pivotOffsetY);
        }
        handleSizeChanged() {
            this._displayObject.size(this._width, this._height);
        }
        handleScaleChanged() {
            this._displayObject.scale(this._scaleX, this._scaleY, true);
        }
        handleGrayedChanged() {
            fgui.ToolSet.setColorFilter(this._displayObject, this._grayed);
        }
        handleAlphaChanged() {
            this._displayObject.alpha = this._alpha;
        }
        handleVisibleChanged() {
            this._displayObject.visible = this.internalVisible2;
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Text:
                    return this.text;
                case fgui.ObjectPropID.Icon:
                    return this.icon;
                case fgui.ObjectPropID.Color:
                    return null;
                case fgui.ObjectPropID.OutlineColor:
                    return null;
                case fgui.ObjectPropID.Playing:
                    return false;
                case fgui.ObjectPropID.Frame:
                    return 0;
                case fgui.ObjectPropID.DeltaTime:
                    return 0;
                case fgui.ObjectPropID.TimeScale:
                    return 1;
                case fgui.ObjectPropID.FontSize:
                    return 0;
                case fgui.ObjectPropID.Selected:
                    return false;
                default:
                    return undefined;
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Text:
                    this.text = value;
                    break;
                case fgui.ObjectPropID.Icon:
                    this.icon = value;
                    break;
            }
        }
        constructFromResource() {
        }
        setup_beforeAdd(buffer, beginPos) {
            buffer.seek(beginPos, 0);
            buffer.skip(5);
            var f1;
            var f2;
            this._id = buffer.readS();
            this._name = buffer.readS();
            f1 = buffer.getInt32();
            f2 = buffer.getInt32();
            this.setXY(f1, f2);
            if (buffer.readBool()) {
                this.initWidth = buffer.getInt32();
                this.initHeight = buffer.getInt32();
                this.setSize(this.initWidth, this.initHeight, true);
            }
            if (buffer.readBool()) {
                this.minWidth = buffer.getInt32();
                this.maxWidth = buffer.getInt32();
                this.minHeight = buffer.getInt32();
                this.maxHeight = buffer.getInt32();
            }
            if (buffer.readBool()) {
                f1 = buffer.getFloat32();
                f2 = buffer.getFloat32();
                this.setScale(f1, f2);
            }
            if (buffer.readBool()) {
                f1 = buffer.getFloat32();
                f2 = buffer.getFloat32();
                this.setSkew(f1, f2);
            }
            if (buffer.readBool()) {
                f1 = buffer.getFloat32();
                f2 = buffer.getFloat32();
                this.setPivot(f1, f2, buffer.readBool());
            }
            f1 = buffer.getFloat32();
            if (f1 != 1)
                this.alpha = f1;
            f1 = buffer.getFloat32();
            if (f1 != 0)
                this.rotation = f1;
            if (!buffer.readBool())
                this.visible = false;
            if (!buffer.readBool())
                this.touchable = false;
            if (buffer.readBool())
                this.grayed = true;
            var bm = buffer.readByte();
            if (fgui.BlendMode[bm])
                this.blendMode = fgui.BlendMode[bm];
            var filter = buffer.readByte();
            if (filter == 1) {
                fgui.ToolSet.setColorFilter(this._displayObject, [buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32()]);
            }
            var str = buffer.readS();
            if (str != null)
                this.data = str;
        }
        setup_afterAdd(buffer, beginPos) {
            buffer.seek(beginPos, 1);
            var str = buffer.readS();
            if (str != null)
                this.tooltips = str;
            var groupId = buffer.getInt16();
            if (groupId >= 0)
                this.group = this.parent.getChildAt(groupId);
            buffer.seek(beginPos, 2);
            var cnt = buffer.getInt16();
            for (var i = 0; i < cnt; i++) {
                var nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                var gear = this.getGear(buffer.readByte());
                gear.setup(buffer);
                buffer.pos = nextPos;
            }
        }
        //drag support
        //-------------------------------------------------------------------
        initDrag() {
            if (this._draggable)
                this.on(Laya.Event.MOUSE_DOWN, this, this.__begin);
            else
                this.off(Laya.Event.MOUSE_DOWN, this, this.__begin);
        }
        dragBegin(touchID) {
            if (GObject.draggingObject) {
                let tmp = GObject.draggingObject;
                tmp.stopDrag();
                GObject.draggingObject = null;
                fgui.Events.dispatch(fgui.Events.DRAG_END, tmp._displayObject, { touchId: touchID });
            }
            sGlobalDragStart.x = Laya.stage.mouseX;
            sGlobalDragStart.y = Laya.stage.mouseY;
            this.localToGlobalRect(0, 0, this.width, this.height, sGlobalRect);
            this._dragTesting = true;
            GObject.draggingObject = this;
            Laya.stage.on(Laya.Event.MOUSE_MOVE, this, this.__moving);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__end);
        }
        dragEnd() {
            if (GObject.draggingObject == this) {
                this.reset();
                this._dragTesting = false;
                GObject.draggingObject = null;
            }
            sDraggingQuery = false;
        }
        reset() {
            Laya.stage.off(Laya.Event.MOUSE_MOVE, this, this.__moving);
            Laya.stage.off(Laya.Event.MOUSE_UP, this, this.__end);
        }
        __begin() {
            if (!this._dragStartPos)
                this._dragStartPos = new Laya.Point();
            this._dragStartPos.x = Laya.stage.mouseX;
            this._dragStartPos.y = Laya.stage.mouseY;
            this._dragTesting = true;
            Laya.stage.on(Laya.Event.MOUSE_MOVE, this, this.__moving);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__end);
        }
        __moving(evt) {
            if (GObject.draggingObject != this && this._draggable && this._dragTesting) {
                var sensitivity = fgui.UIConfig.touchDragSensitivity;
                if (this._dragStartPos
                    && Math.abs(this._dragStartPos.x - Laya.stage.mouseX) < sensitivity
                    && Math.abs(this._dragStartPos.y - Laya.stage.mouseY) < sensitivity)
                    return;
                this._dragTesting = false;
                sDraggingQuery = true;
                fgui.Events.dispatch(fgui.Events.DRAG_START, this._displayObject, evt);
                if (sDraggingQuery)
                    this.dragBegin();
            }
            if (GObject.draggingObject == this) {
                var xx = Laya.stage.mouseX - sGlobalDragStart.x + sGlobalRect.x;
                var yy = Laya.stage.mouseY - sGlobalDragStart.y + sGlobalRect.y;
                if (this._dragBounds) {
                    var rect = fgui.GRoot.inst.localToGlobalRect(this._dragBounds.x, this._dragBounds.y, this._dragBounds.width, this._dragBounds.height, sDragHelperRect);
                    if (xx < rect.x)
                        xx = rect.x;
                    else if (xx + sGlobalRect.width > rect.right) {
                        xx = rect.right - sGlobalRect.width;
                        if (xx < rect.x)
                            xx = rect.x;
                    }
                    if (yy < rect.y)
                        yy = rect.y;
                    else if (yy + sGlobalRect.height > rect.bottom) {
                        yy = rect.bottom - sGlobalRect.height;
                        if (yy < rect.y)
                            yy = rect.y;
                    }
                }
                sUpdateInDragging = true;
                var pt = this.parent.globalToLocal(xx, yy, sHelperPoint);
                this.setXY(Math.round(pt.x), Math.round(pt.y));
                sUpdateInDragging = false;
                fgui.Events.dispatch(fgui.Events.DRAG_MOVE, this._displayObject, evt);
            }
        }
        __end(evt) {
            if (GObject.draggingObject == this) {
                GObject.draggingObject = null;
                this.reset();
                fgui.Events.dispatch(fgui.Events.DRAG_END, this._displayObject, evt);
            }
            else if (this._dragTesting) {
                this._dragTesting = false;
                this.reset();
            }
        }
        //-------------------------------------------------------------------
        static cast(sprite) {
            return (sprite["$owner"]);
        }
    }
    fgui.GObject = GObject;
    fgui.BlendMode = {
        2: Laya.BlendMode.LIGHTER,
        //3: Laya.BlendMode.MULTIPLY,
        //4: Laya.BlendMode.SCREEN
    };
    var _gInstanceCounter = 0;
    var sGlobalDragStart = new Laya.Point();
    var sGlobalRect = new Laya.Rectangle();
    var sHelperPoint = new Laya.Point();
    var sDragHelperRect = new Laya.Rectangle();
    var sUpdateInDragging;
    var sDraggingQuery;
})(fgui);

(function (fgui) {
    class Margin {
        constructor() {
            this.left = 0;
            this.right = 0;
            this.top = 0;
            this.bottom = 0;
        }
        copy(source) {
            this.top = source.top;
            this.bottom = source.bottom;
            this.left = source.left;
            this.right = source.right;
        }
    }
    fgui.Margin = Margin;
})(fgui);
///<reference path="GObject.ts"/>
///<reference path="Margin.ts"/>

///<reference path="GObject.ts"/>
///<reference path="Margin.ts"/>
(function (fgui) {
    class GComponent extends fgui.GObject {
        constructor() {
            super();
            this._sortingChildCount = 0;
            this._children = [];
            this._controllers = [];
            this._transitions = [];
            this._margin = new fgui.Margin();
            this._alignOffset = new Laya.Point();
            this._opaque = false;
            this._childrenRenderOrder = 0;
            this._apexIndex = 0;
        }
        createDisplayObject() {
            super.createDisplayObject();
            this._displayObject.mouseEnabled = true;
            this._displayObject.mouseThrough = true;
            this._container = this._displayObject;
        }
        dispose() {
            var i;
            var cnt;
            cnt = this._transitions.length;
            for (i = 0; i < cnt; ++i) {
                var trans = this._transitions[i];
                trans.dispose();
            }
            cnt = this._controllers.length;
            for (i = 0; i < cnt; ++i) {
                var cc = this._controllers[i];
                cc.dispose();
            }
            if (this.scrollPane)
                this.scrollPane.dispose();
            cnt = this._children.length;
            for (i = cnt - 1; i >= 0; --i) {
                var obj = this._children[i];
                obj.parent = null; //avoid removeFromParent call
                obj.dispose();
            }
            this._boundsChanged = false;
            super.dispose();
        }
        get displayListContainer() {
            return this._container;
        }
        addChild(child) {
            this.addChildAt(child, this._children.length);
            return child;
        }
        addChildAt(child, index) {
            if (!child)
                throw new Error("child is null");
            if (index >= 0 && index <= this._children.length) {
                if (child.parent == this) {
                    this.setChildIndex(child, index);
                }
                else {
                    child.removeFromParent();
                    child.parent = this;
                    var cnt = this._children.length;
                    if (child.sortingOrder != 0) {
                        this._sortingChildCount++;
                        index = this.getInsertPosForSortingChild(child);
                    }
                    else if (this._sortingChildCount > 0) {
                        if (index > (cnt - this._sortingChildCount))
                            index = cnt - this._sortingChildCount;
                    }
                    if (index == cnt)
                        this._children.push(child);
                    else
                        this._children.splice(index, 0, child);
                    this.childStateChanged(child);
                    this.setBoundsChangedFlag();
                }
                return child;
            }
            else {
                throw new Error("Invalid child index");
            }
        }
        getInsertPosForSortingChild(target) {
            var cnt = this._children.length;
            var i = 0;
            for (i = 0; i < cnt; i++) {
                var child = this._children[i];
                if (child == target)
                    continue;
                if (target.sortingOrder < child.sortingOrder)
                    break;
            }
            return i;
        }
        removeChild(child, dispose) {
            var childIndex = this._children.indexOf(child);
            if (childIndex != -1) {
                this.removeChildAt(childIndex, dispose);
            }
            return child;
        }
        removeChildAt(index, dispose) {
            if (index >= 0 && index < this._children.length) {
                var child = this._children[index];
                child.parent = null;
                if (child.sortingOrder != 0)
                    this._sortingChildCount--;
                this._children.splice(index, 1);
                child.group = null;
                if (child.inContainer) {
                    this._container.removeChild(child.displayObject);
                    if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Arch)
                        Laya.timer.callLater(this, this.buildNativeDisplayList);
                }
                if (dispose)
                    child.dispose();
                this.setBoundsChangedFlag();
                return child;
            }
            else {
                throw new Error("Invalid child index");
            }
        }
        removeChildren(beginIndex, endIndex, dispose) {
            if (beginIndex == undefined)
                beginIndex = 0;
            if (endIndex == undefined)
                endIndex = -1;
            if (endIndex < 0 || endIndex >= this._children.length)
                endIndex = this._children.length - 1;
            for (var i = beginIndex; i <= endIndex; ++i)
                this.removeChildAt(beginIndex, dispose);
        }
        getChildAt(index, classType) {
            if (index >= 0 && index < this._children.length)
                return this._children[index];
            else
                throw new Error("Invalid child index");
        }
        getChild(name, classType) {
            var cnt = this._children.length;
            for (var i = 0; i < cnt; ++i) {
                if (this._children[i].name == name)
                    return this._children[i];
            }
            return null;
        }
        getChildByPath(path, classType) {
            var arr = path.split(".");
            var cnt = arr.length;
            var gcom = this;
            var obj;
            for (var i = 0; i < cnt; ++i) {
                obj = gcom.getChild(arr[i]);
                if (!obj)
                    break;
                if (i != cnt - 1) {
                    if (!(obj instanceof GComponent)) {
                        obj = null;
                        break;
                    }
                    else
                        gcom = obj;
                }
            }
            return obj;
        }
        getVisibleChild(name) {
            var cnt = this._children.length;
            for (var i = 0; i < cnt; ++i) {
                var child = this._children[i];
                if (child.internalVisible && child.internalVisible2 && child.name == name)
                    return child;
            }
            return null;
        }
        getChildInGroup(name, group) {
            var cnt = this._children.length;
            for (var i = 0; i < cnt; ++i) {
                var child = this._children[i];
                if (child.group == group && child.name == name)
                    return child;
            }
            return null;
        }
        getChildById(id) {
            var cnt = this._children.length;
            for (var i = 0; i < cnt; ++i) {
                if (this._children[i]._id == id)
                    return this._children[i];
            }
            return null;
        }
        getChildIndex(child) {
            return this._children.indexOf(child);
        }
        setChildIndex(child, index) {
            var oldIndex = this._children.indexOf(child);
            if (oldIndex == -1)
                throw new Error("Not a child of this container");
            if (child.sortingOrder != 0) //no effect
                return;
            var cnt = this._children.length;
            if (this._sortingChildCount > 0) {
                if (index > (cnt - this._sortingChildCount - 1))
                    index = cnt - this._sortingChildCount - 1;
            }
            this._setChildIndex(child, oldIndex, index);
        }
        setChildIndexBefore(child, index) {
            var oldIndex = this._children.indexOf(child);
            if (oldIndex == -1)
                throw new Error("Not a child of this container");
            if (child.sortingOrder != 0) //no effect
                return oldIndex;
            var cnt = this._children.length;
            if (this._sortingChildCount > 0) {
                if (index > (cnt - this._sortingChildCount - 1))
                    index = cnt - this._sortingChildCount - 1;
            }
            if (oldIndex < index)
                return this._setChildIndex(child, oldIndex, index - 1);
            else
                return this._setChildIndex(child, oldIndex, index);
        }
        _setChildIndex(child, oldIndex, index) {
            var cnt = this._children.length;
            if (index > cnt)
                index = cnt;
            if (oldIndex == index)
                return oldIndex;
            this._children.splice(oldIndex, 1);
            this._children.splice(index, 0, child);
            if (child.inContainer) {
                var displayIndex = 0;
                var g;
                var i;
                if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Ascent) {
                    for (i = 0; i < index; i++) {
                        g = this._children[i];
                        if (g.inContainer)
                            displayIndex++;
                    }
                    if (displayIndex == this._container.numChildren)
                        displayIndex--;
                    this._container.setChildIndex(child.displayObject, displayIndex);
                }
                else if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Descent) {
                    for (i = cnt - 1; i > index; i--) {
                        g = this._children[i];
                        if (g.inContainer)
                            displayIndex++;
                    }
                    if (displayIndex == this._container.numChildren)
                        displayIndex--;
                    this._container.setChildIndex(child.displayObject, displayIndex);
                }
                else {
                    Laya.timer.callLater(this, this.buildNativeDisplayList);
                }
                this.setBoundsChangedFlag();
            }
            return index;
        }
        swapChildren(child1, child2) {
            var index1 = this._children.indexOf(child1);
            var index2 = this._children.indexOf(child2);
            if (index1 == -1 || index2 == -1)
                throw new Error("Not a child of this container");
            this.swapChildrenAt(index1, index2);
        }
        swapChildrenAt(index1, index2) {
            var child1 = this._children[index1];
            var child2 = this._children[index2];
            this.setChildIndex(child1, index2);
            this.setChildIndex(child2, index1);
        }
        get numChildren() {
            return this._children.length;
        }
        isAncestorOf(child) {
            if (!child)
                return false;
            var p = child.parent;
            while (p) {
                if (p == this)
                    return true;
                p = p.parent;
            }
            return false;
        }
        addController(controller) {
            this._controllers.push(controller);
            controller.parent = this;
            this.applyController(controller);
        }
        getControllerAt(index) {
            return this._controllers[index];
        }
        getController(name) {
            var cnt = this._controllers.length;
            for (var i = 0; i < cnt; ++i) {
                var c = this._controllers[i];
                if (c.name == name)
                    return c;
            }
            return null;
        }
        removeController(c) {
            var index = this._controllers.indexOf(c);
            if (index == -1)
                throw new Error("controller not exists");
            c.parent = null;
            this._controllers.splice(index, 1);
            var length = this._children.length;
            for (var i = 0; i < length; i++) {
                var child = this._children[i];
                child.handleControllerChanged(c);
            }
        }
        get controllers() {
            return this._controllers;
        }
        childStateChanged(child) {
            if (this._buildingDisplayList)
                return;
            var cnt = this._children.length;
            if (child instanceof fgui.GGroup) {
                for (var i = 0; i < cnt; i++) {
                    var g = this._children[i];
                    if (g.group == child)
                        this.childStateChanged(g);
                }
                return;
            }
            if (!child.displayObject)
                return;
            if (child.internalVisible && child.displayObject != this._displayObject.mask) {
                if (!child.displayObject.parent) {
                    var index = 0;
                    if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Ascent) {
                        for (i = 0; i < cnt; i++) {
                            g = this._children[i];
                            if (g == child)
                                break;
                            if (g.displayObject && g.displayObject.parent)
                                index++;
                        }
                        this._container.addChildAt(child.displayObject, index);
                    }
                    else if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Descent) {
                        for (i = cnt - 1; i >= 0; i--) {
                            g = this._children[i];
                            if (g == child)
                                break;
                            if (g.displayObject && g.displayObject.parent)
                                index++;
                        }
                        this._container.addChildAt(child.displayObject, index);
                    }
                    else {
                        this._container.addChild(child.displayObject);
                        Laya.timer.callLater(this, this.buildNativeDisplayList);
                    }
                }
            }
            else {
                if (child.displayObject.parent) {
                    this._container.removeChild(child.displayObject);
                    if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Arch)
                        Laya.timer.callLater(this, this.buildNativeDisplayList);
                }
            }
        }
        buildNativeDisplayList() {
            if (!this._displayObject)
                return;
            var cnt = this._children.length;
            if (cnt == 0)
                return;
            var i;
            var child;
            switch (this._childrenRenderOrder) {
                case fgui.ChildrenRenderOrder.Ascent:
                    {
                        for (i = 0; i < cnt; i++) {
                            child = this._children[i];
                            if (child.displayObject && child.internalVisible)
                                this._container.addChild(child.displayObject);
                        }
                    }
                    break;
                case fgui.ChildrenRenderOrder.Descent:
                    {
                        for (i = cnt - 1; i >= 0; i--) {
                            child = this._children[i];
                            if (child.displayObject && child.internalVisible)
                                this._container.addChild(child.displayObject);
                        }
                    }
                    break;
                case fgui.ChildrenRenderOrder.Arch:
                    {
                        var apex = fgui.ToolSet.clamp(this._apexIndex, 0, cnt);
                        for (i = 0; i < apex; i++) {
                            child = this._children[i];
                            if (child.displayObject && child.internalVisible)
                                this._container.addChild(child.displayObject);
                        }
                        for (i = cnt - 1; i >= apex; i--) {
                            child = this._children[i];
                            if (child.displayObject && child.internalVisible)
                                this._container.addChild(child.displayObject);
                        }
                    }
                    break;
            }
        }
        applyController(c) {
            this._applyingController = c;
            var child;
            var length = this._children.length;
            for (var i = 0; i < length; i++) {
                child = this._children[i];
                child.handleControllerChanged(c);
            }
            this._applyingController = null;
            c.runActions();
        }
        applyAllControllers() {
            var cnt = this._controllers.length;
            for (var i = 0; i < cnt; ++i) {
                this.applyController(this._controllers[i]);
            }
        }
        adjustRadioGroupDepth(obj, c) {
            var cnt = this._children.length;
            var i;
            var child;
            var myIndex = -1, maxIndex = -1;
            for (i = 0; i < cnt; i++) {
                child = this._children[i];
                if (child == obj) {
                    myIndex = i;
                }
                else if ((child instanceof fgui.GButton)
                    && (child).relatedController == c) {
                    if (i > maxIndex)
                        maxIndex = i;
                }
            }
            if (myIndex < maxIndex) {
                //如果正在applyingController，此时修改显示列表是危险的，但真正排除危险只能用显示列表的副本去做，这样性能可能损耗较大，
                //这里取个巧，让可能漏过的child补一下handleControllerChanged，反正重复执行是无害的。
                if (this._applyingController)
                    this._children[maxIndex].handleControllerChanged(this._applyingController);
                this.swapChildrenAt(myIndex, maxIndex);
            }
        }
        getTransitionAt(index) {
            return this._transitions[index];
        }
        getTransition(transName) {
            var cnt = this._transitions.length;
            for (var i = 0; i < cnt; ++i) {
                var trans = this._transitions[i];
                if (trans.name == transName)
                    return trans;
            }
            return null;
        }
        isChildInView(child) {
            if (this._displayObject.scrollRect) {
                return child.x + child.width >= 0 && child.x <= this.width
                    && child.y + child.height >= 0 && child.y <= this.height;
            }
            else if (this._scrollPane) {
                return this._scrollPane.isChildInView(child);
            }
            else
                return true;
        }
        getFirstChildInView() {
            var cnt = this._children.length;
            for (var i = 0; i < cnt; ++i) {
                var child = this._children[i];
                if (this.isChildInView(child))
                    return i;
            }
            return -1;
        }
        get scrollPane() {
            return this._scrollPane;
        }
        get opaque() {
            return this._opaque;
        }
        set opaque(value) {
            if (this._opaque != value) {
                this._opaque = value;
                if (this._opaque) {
                    if (!this._displayObject.hitArea)
                        this._displayObject.hitArea = new Laya.Rectangle();
                    if (this._displayObject.hitArea instanceof Laya.Rectangle)
                        this._displayObject.hitArea.setTo(0, 0, this._width, this._height);
                    this._displayObject.mouseThrough = false;
                }
                else {
                    if (this._displayObject.hitArea instanceof Laya.Rectangle)
                        this._displayObject.hitArea = null;
                    this._displayObject.mouseThrough = true;
                }
            }
        }
        get margin() {
            return this._margin;
        }
        set margin(value) {
            this._margin.copy(value);
            if (this._displayObject.scrollRect) {
                this._container.pos(this._margin.left + this._alignOffset.x, this._margin.top + this._alignOffset.y);
            }
            this.handleSizeChanged();
        }
        get childrenRenderOrder() {
            return this._childrenRenderOrder;
        }
        set childrenRenderOrder(value) {
            if (this._childrenRenderOrder != value) {
                this._childrenRenderOrder = value;
                this.buildNativeDisplayList();
            }
        }
        get apexIndex() {
            return this._apexIndex;
        }
        set apexIndex(value) {
            if (this._apexIndex != value) {
                this._apexIndex = value;
                if (this._childrenRenderOrder == fgui.ChildrenRenderOrder.Arch)
                    this.buildNativeDisplayList();
            }
        }
        get mask() {
            return this._mask;
        }
        set mask(value) {
            this.setMask(value, false);
        }
        setMask(value, reversed) {
            if (this._mask && this._mask != value) {
                if (this._mask.blendMode == "destination-out")
                    this._mask.blendMode = null;
            }
            this._mask = value;
            if (!this._mask) {
                this._displayObject.mask = null;
                if (this._displayObject.hitArea instanceof fgui.ChildHitArea)
                    this._displayObject.hitArea = null;
                return;
            }
            if (this._mask.hitArea) {
                this._displayObject.hitArea = new fgui.ChildHitArea(this._mask, reversed);
                this._displayObject.mouseThrough = false;
                this._displayObject.hitTestPrior = true;
            }
            if (reversed) {
                this._displayObject.mask = null;
                this._displayObject.cacheAs = "bitmap";
                this._mask.blendMode = "destination-out";
            }
            else
                this._displayObject.mask = this._mask;
        }
        get baseUserData() {
            var buffer = this.packageItem.rawData;
            buffer.seek(0, 4);
            return buffer.readS();
        }
        updateHitArea() {
            if (this._displayObject.hitArea instanceof fgui.PixelHitTest) {
                var hitTest = (this._displayObject.hitArea);
                if (this.sourceWidth != 0)
                    hitTest.scaleX = this._width / this.sourceWidth;
                if (this.sourceHeight != 0)
                    hitTest.scaleY = this._height / this.sourceHeight;
            }
            else if (this._displayObject.hitArea instanceof Laya.Rectangle) {
                this._displayObject.hitArea.setTo(0, 0, this._width, this._height);
            }
        }
        updateMask() {
            var rect = this._displayObject.scrollRect;
            if (!rect)
                rect = new Laya.Rectangle();
            rect.x = this._margin.left;
            rect.y = this._margin.top;
            rect.width = this._width - this._margin.right;
            rect.height = this._height - this._margin.bottom;
            this._displayObject.scrollRect = rect;
        }
        setupScroll(buffer) {
            if (this._displayObject == this._container) {
                this._container = new Laya.Sprite();
                this._displayObject.addChild(this._container);
            }
            this._scrollPane = new fgui.ScrollPane(this);
            this._scrollPane.setup(buffer);
        }
        setupOverflow(overflow) {
            if (overflow == fgui.OverflowType.Hidden) {
                if (this._displayObject == this._container) {
                    this._container = new Laya.Sprite();
                    this._displayObject.addChild(this._container);
                }
                this.updateMask();
                this._container.pos(this._margin.left, this._margin.top);
            }
            else if (this._margin.left != 0 || this._margin.top != 0) {
                if (this._displayObject == this._container) {
                    this._container = new Laya.Sprite();
                    this._displayObject.addChild(this._container);
                }
                this._container.pos(this._margin.left, this._margin.top);
            }
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            if (this._scrollPane)
                this._scrollPane.onOwnerSizeChanged();
            else if (this._displayObject.scrollRect)
                this.updateMask();
            if (this._displayObject.hitArea)
                this.updateHitArea();
        }
        handleGrayedChanged() {
            var c = this.getController("grayed");
            if (c) {
                c.selectedIndex = this.grayed ? 1 : 0;
                return;
            }
            var v = this.grayed;
            var cnt = this._children.length;
            for (var i = 0; i < cnt; ++i) {
                this._children[i].grayed = v;
            }
        }
        handleControllerChanged(c) {
            super.handleControllerChanged(c);
            if (this._scrollPane)
                this._scrollPane.handleControllerChanged(c);
        }
        setBoundsChangedFlag() {
            if (!this._scrollPane && !this._trackBounds)
                return;
            if (!this._boundsChanged) {
                this._boundsChanged = true;
                Laya.timer.callLater(this, this.__render);
            }
        }
        __render() {
            if (this._boundsChanged) {
                var i1 = 0;
                var len = this._children.length;
                var child;
                for (i1 = 0; i1 < len; i1++) {
                    child = this._children[i1];
                    child.ensureSizeCorrect();
                }
                this.updateBounds();
            }
        }
        ensureBoundsCorrect() {
            var i1 = 0;
            var len = this._children.length;
            var child;
            for (i1 = 0; i1 < len; i1++) {
                child = this._children[i1];
                child.ensureSizeCorrect();
            }
            if (this._boundsChanged)
                this.updateBounds();
        }
        updateBounds() {
            var ax = 0, ay = 0, aw = 0, ah = 0;
            var len = this._children.length;
            if (len > 0) {
                ax = Number.POSITIVE_INFINITY, ay = Number.POSITIVE_INFINITY;
                var ar = Number.NEGATIVE_INFINITY, ab = Number.NEGATIVE_INFINITY;
                var tmp = 0;
                var i1 = 0;
                for (i1 = 0; i1 < len; i1++) {
                    var child = this._children[i1];
                    tmp = child.x;
                    if (tmp < ax)
                        ax = tmp;
                    tmp = child.y;
                    if (tmp < ay)
                        ay = tmp;
                    tmp = child.x + child.actualWidth;
                    if (tmp > ar)
                        ar = tmp;
                    tmp = child.y + child.actualHeight;
                    if (tmp > ab)
                        ab = tmp;
                }
                aw = ar - ax;
                ah = ab - ay;
            }
            this.setBounds(ax, ay, aw, ah);
        }
        setBounds(ax, ay, aw, ah) {
            this._boundsChanged = false;
            if (this._scrollPane)
                this._scrollPane.setContentSize(Math.round(ax + aw), Math.round(ay + ah));
        }
        get viewWidth() {
            if (this._scrollPane)
                return this._scrollPane.viewWidth;
            else
                return this.width - this._margin.left - this._margin.right;
        }
        set viewWidth(value) {
            if (this._scrollPane)
                this._scrollPane.viewWidth = value;
            else
                this.width = value + this._margin.left + this._margin.right;
        }
        get viewHeight() {
            if (this._scrollPane)
                return this._scrollPane.viewHeight;
            else
                return this.height - this._margin.top - this._margin.bottom;
        }
        set viewHeight(value) {
            if (this._scrollPane)
                this._scrollPane.viewHeight = value;
            else
                this.height = value + this._margin.top + this._margin.bottom;
        }
        getSnappingPosition(xValue, yValue, result) {
            return this.getSnappingPositionWithDir(xValue, yValue, 0, 0, result);
        }
        /**
         * dir正数表示右移或者下移，负数表示左移或者上移
         */
        getSnappingPositionWithDir(xValue, yValue, xDir, yDir, result) {
            if (!result)
                result = new Laya.Point();
            var cnt = this._children.length;
            if (cnt == 0) {
                result.x = 0;
                result.y = 0;
                return result;
            }
            this.ensureBoundsCorrect();
            var obj = null;
            var prev = null;
            var i = 0;
            if (yValue != 0) {
                for (; i < cnt; i++) {
                    obj = this._children[i];
                    if (yValue < obj.y) {
                        if (i == 0) {
                            yValue = 0;
                            break;
                        }
                        else {
                            prev = this._children[i - 1];
                            if (yValue < prev.y + prev.actualHeight / 2) //top half part
                                yValue = prev.y;
                            else //bottom half part
                                yValue = obj.y;
                            break;
                        }
                    }
                }
                if (i == cnt)
                    yValue = obj.y;
            }
            if (xValue != 0) {
                if (i > 0)
                    i--;
                for (; i < cnt; i++) {
                    obj = this._children[i];
                    if (xValue < obj.x) {
                        if (i == 0) {
                            xValue = 0;
                            break;
                        }
                        else {
                            prev = this._children[i - 1];
                            if (xValue < prev.x + prev.actualWidth / 2) //top half part
                                xValue = prev.x;
                            else //bottom half part
                                xValue = obj.x;
                            break;
                        }
                    }
                }
                if (i == cnt)
                    xValue = obj.x;
            }
            result.x = xValue;
            result.y = yValue;
            return result;
        }
        childSortingOrderChanged(child, oldValue, newValue) {
            if (newValue == 0) {
                this._sortingChildCount--;
                this.setChildIndex(child, this._children.length);
            }
            else {
                if (oldValue == 0)
                    this._sortingChildCount++;
                var oldIndex = this._children.indexOf(child);
                var index = this.getInsertPosForSortingChild(child);
                if (oldIndex < index)
                    this._setChildIndex(child, oldIndex, index - 1);
                else
                    this._setChildIndex(child, oldIndex, index);
            }
        }
        constructFromResource() {
            this.constructFromResource2(null, 0);
        }
        constructFromResource2(objectPool, poolIndex) {
            var contentItem = this.packageItem.getBranch();
            if (!contentItem.decoded) {
                contentItem.decoded = true;
                fgui.TranslationHelper.translateComponent(contentItem);
            }
            var i;
            var dataLen;
            var curPos;
            var nextPos;
            var f1;
            var f2;
            var i1;
            var i2;
            var buffer = contentItem.rawData;
            buffer.seek(0, 0);
            this._underConstruct = true;
            this.sourceWidth = buffer.getInt32();
            this.sourceHeight = buffer.getInt32();
            this.initWidth = this.sourceWidth;
            this.initHeight = this.sourceHeight;
            this.setSize(this.sourceWidth, this.sourceHeight);
            if (buffer.readBool()) {
                this.minWidth = buffer.getInt32();
                this.maxWidth = buffer.getInt32();
                this.minHeight = buffer.getInt32();
                this.maxHeight = buffer.getInt32();
            }
            if (buffer.readBool()) {
                f1 = buffer.getFloat32();
                f2 = buffer.getFloat32();
                this.internalSetPivot(f1, f2, buffer.readBool());
            }
            if (buffer.readBool()) {
                this._margin.top = buffer.getInt32();
                this._margin.bottom = buffer.getInt32();
                this._margin.left = buffer.getInt32();
                this._margin.right = buffer.getInt32();
            }
            var overflow = buffer.readByte();
            if (overflow == fgui.OverflowType.Scroll) {
                var savedPos = buffer.pos;
                buffer.seek(0, 7);
                this.setupScroll(buffer);
                buffer.pos = savedPos;
            }
            else
                this.setupOverflow(overflow);
            if (buffer.readBool())
                buffer.skip(8);
            this._buildingDisplayList = true;
            buffer.seek(0, 1);
            var controllerCount = buffer.getInt16();
            for (i = 0; i < controllerCount; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                var controller = new fgui.Controller();
                this._controllers.push(controller);
                controller.parent = this;
                controller.setup(buffer);
                buffer.pos = nextPos;
            }
            buffer.seek(0, 2);
            var child;
            var childCount = buffer.getInt16();
            for (i = 0; i < childCount; i++) {
                dataLen = buffer.getInt16();
                curPos = buffer.pos;
                if (objectPool)
                    child = objectPool[poolIndex + i];
                else {
                    buffer.seek(curPos, 0);
                    var type = buffer.readByte();
                    var src = buffer.readS();
                    var pkgId = buffer.readS();
                    var pi = null;
                    if (src != null) {
                        var pkg;
                        if (pkgId != null)
                            pkg = fgui.UIPackage.getById(pkgId);
                        else
                            pkg = contentItem.owner;
                        pi = pkg ? pkg.getItemById(src) : null;
                    }
                    if (pi) {
                        child = fgui.UIObjectFactory.newObject(pi);
                        child.constructFromResource();
                    }
                    else
                        child = fgui.UIObjectFactory.newObject(type);
                }
                child._underConstruct = true;
                child.setup_beforeAdd(buffer, curPos);
                child.parent = this;
                this._children.push(child);
                buffer.pos = curPos + dataLen;
            }
            buffer.seek(0, 3);
            this.relations.setup(buffer, true);
            buffer.seek(0, 2);
            buffer.skip(2);
            for (i = 0; i < childCount; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                buffer.seek(buffer.pos, 3);
                this._children[i].relations.setup(buffer, false);
                buffer.pos = nextPos;
            }
            buffer.seek(0, 2);
            buffer.skip(2);
            for (i = 0; i < childCount; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                child = this._children[i];
                child.setup_afterAdd(buffer, buffer.pos);
                child._underConstruct = false;
                buffer.pos = nextPos;
            }
            buffer.seek(0, 4);
            buffer.skip(2); //customData
            this.opaque = buffer.readBool();
            var maskId = buffer.getInt16();
            if (maskId != -1) {
                this.setMask(this.getChildAt(maskId).displayObject, buffer.readBool());
            }
            var hitTestId = buffer.readS();
            i1 = buffer.getInt32();
            i2 = buffer.getInt32();
            var hitArea;
            if (hitTestId) {
                pi = contentItem.owner.getItemById(hitTestId);
                if (pi && pi.pixelHitTestData)
                    hitArea = new fgui.PixelHitTest(pi.pixelHitTestData, i1, i2);
            }
            else if (i1 != 0 && i2 != -1) {
                hitArea = new fgui.ChildHitArea(this.getChildAt(i2).displayObject);
            }
            if (hitArea) {
                this._displayObject.hitArea = hitArea;
                this._displayObject.mouseThrough = false;
                this._displayObject.hitTestPrior = true;
            }
            buffer.seek(0, 5);
            var transitionCount = buffer.getInt16();
            for (i = 0; i < transitionCount; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                var trans = new fgui.Transition(this);
                trans.setup(buffer);
                this._transitions.push(trans);
                buffer.pos = nextPos;
            }
            if (this._transitions.length > 0) {
                this.displayObject.on(Laya.Event.DISPLAY, this, this.___added);
                this.displayObject.on(Laya.Event.UNDISPLAY, this, this.___removed);
            }
            this.applyAllControllers();
            this._buildingDisplayList = false;
            this._underConstruct = false;
            this.buildNativeDisplayList();
            this.setBoundsChangedFlag();
            if (contentItem.objectType != fgui.ObjectType.Component)
                this.constructExtension(buffer);
            this.onConstruct();
        }
        constructExtension(buffer) {
        }
        onConstruct() {
            this.constructFromXML(null); //old version
        }
        constructFromXML(xml) {
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            buffer.seek(beginPos, 4);
            var pageController = buffer.getInt16();
            if (pageController != -1 && this._scrollPane)
                this._scrollPane.pageController = this._parent.getControllerAt(pageController);
            var cnt;
            var i;
            cnt = buffer.getInt16();
            for (i = 0; i < cnt; i++) {
                var cc = this.getController(buffer.readS());
                var pageId = buffer.readS();
                if (cc)
                    cc.selectedPageId = pageId;
            }
            if (buffer.version >= 2) {
                cnt = buffer.getInt16();
                for (i = 0; i < cnt; i++) {
                    var target = buffer.readS();
                    var propertyId = buffer.getInt16();
                    var value = buffer.readS();
                    var obj = this.getChildByPath(target);
                    if (obj)
                        obj.setProp(propertyId, value);
                }
            }
        }
        ___added() {
            var cnt = this._transitions.length;
            for (var i = 0; i < cnt; ++i) {
                this._transitions[i].onOwnerAddedToStage();
            }
        }
        ___removed() {
            var cnt = this._transitions.length;
            for (var i = 0; i < cnt; ++i) {
                this._transitions[i].onOwnerRemovedFromStage();
            }
        }
    }
    fgui.GComponent = GComponent;
})(fgui);
///<reference path="GComponent.ts"/>

///<reference path="GComponent.ts"/>
(function (fgui) {
    class GButton extends fgui.GComponent {
        constructor() {
            super();
            this._soundVolumeScale = 0;
            this._downEffect = 0;
            this._mode = fgui.ButtonMode.Common;
            this._title = "";
            this._icon = "";
            this._sound = fgui.UIConfig.buttonSound;
            this._soundVolumeScale = fgui.UIConfig.buttonSoundVolumeScale;
            this._changeStateOnClick = true;
            this._downEffectValue = 0.8;
        }
        get icon() {
            return this._icon;
        }
        set icon(value) {
            this._icon = value;
            value = (this._selected && this._selectedIcon) ? this._selectedIcon : this._icon;
            if (this._iconObject)
                this._iconObject.icon = value;
            this.updateGear(7);
        }
        get selectedIcon() {
            return this._selectedIcon;
        }
        set selectedIcon(value) {
            this._selectedIcon = value;
            value = (this._selected && this._selectedIcon) ? this._selectedIcon : this._icon;
            if (this._iconObject)
                this._iconObject.icon = value;
        }
        get title() {
            return this._title;
        }
        set title(value) {
            this._title = value;
            if (this._titleObject)
                this._titleObject.text = (this._selected && this._selectedTitle) ? this._selectedTitle : this._title;
            this.updateGear(6);
        }
        get text() {
            return this.title;
        }
        set text(value) {
            this.title = value;
        }
        get selectedTitle() {
            return this._selectedTitle;
        }
        set selectedTitle(value) {
            this._selectedTitle = value;
            if (this._titleObject)
                this._titleObject.text = (this._selected && this._selectedTitle) ? this._selectedTitle : this._title;
        }
        get titleColor() {
            var tf = this.getTextField();
            if (tf)
                return tf.color;
            else
                return "#000000";
        }
        set titleColor(value) {
            var tf = this.getTextField();
            if (tf)
                tf.color = value;
            this.updateGear(4);
        }
        get titleFontSize() {
            var tf = this.getTextField();
            if (tf)
                return tf.fontSize;
            else
                return 0;
        }
        set titleFontSize(value) {
            var tf = this.getTextField();
            if (tf)
                tf.fontSize = value;
        }
        get sound() {
            return this._sound;
        }
        set sound(val) {
            this._sound = val;
        }
        get soundVolumeScale() {
            return this._soundVolumeScale;
        }
        set soundVolumeScale(value) {
            this._soundVolumeScale = value;
        }
        set selected(val) {
            if (this._mode == fgui.ButtonMode.Common)
                return;
            if (this._selected != val) {
                this._selected = val;
                if (this.grayed && this._buttonController && this._buttonController.hasPage(GButton.DISABLED)) {
                    if (this._selected)
                        this.setState(GButton.SELECTED_DISABLED);
                    else
                        this.setState(GButton.DISABLED);
                }
                else {
                    if (this._selected)
                        this.setState(this._over ? GButton.SELECTED_OVER : GButton.DOWN);
                    else
                        this.setState(this._over ? GButton.OVER : GButton.UP);
                }
                if (this._selectedTitle && this._titleObject)
                    this._titleObject.text = this._selected ? this._selectedTitle : this._title;
                if (this._selectedIcon) {
                    var str = this._selected ? this._selectedIcon : this._icon;
                    if (this._iconObject)
                        this._iconObject.icon = str;
                }
                if (this._relatedController
                    && this._parent
                    && !this._parent._buildingDisplayList) {
                    if (this._selected) {
                        this._relatedController.selectedPageId = this._relatedPageId;
                        if (this._relatedController.autoRadioGroupDepth)
                            this._parent.adjustRadioGroupDepth(this, this._relatedController);
                    }
                    else if (this._mode == fgui.ButtonMode.Check && this._relatedController.selectedPageId == this._relatedPageId)
                        this._relatedController.oppositePageId = this._relatedPageId;
                }
            }
        }
        get selected() {
            return this._selected;
        }
        get mode() {
            return this._mode;
        }
        set mode(value) {
            if (this._mode != value) {
                if (value == fgui.ButtonMode.Common)
                    this.selected = false;
                this._mode = value;
            }
        }
        get relatedController() {
            return this._relatedController;
        }
        set relatedController(val) {
            if (val != this._relatedController) {
                this._relatedController = val;
                this._relatedPageId = null;
            }
        }
        get relatedPageId() {
            return this._relatedPageId;
        }
        set relatedPageId(val) {
            this._relatedPageId = val;
        }
        get changeStateOnClick() {
            return this._changeStateOnClick;
        }
        set changeStateOnClick(value) {
            this._changeStateOnClick = value;
        }
        get linkedPopup() {
            return this._linkedPopup;
        }
        set linkedPopup(value) {
            this._linkedPopup = value;
        }
        getTextField() {
            if (this._titleObject instanceof fgui.GTextField)
                return this._titleObject;
            else if ((this._titleObject instanceof fgui.GLabel) || (this._titleObject instanceof GButton))
                return this._titleObject.getTextField();
            else
                return null;
        }
        fireClick(downEffect) {
            if (downEffect == null)
                downEffect = true;
            if (downEffect && this._mode == fgui.ButtonMode.Common) {
                this.setState(GButton.OVER);
                Laya.timer.once(100, this, this.setState, [GButton.DOWN], false);
                Laya.timer.once(200, this, this.setState, [GButton.UP], false);
            }
            this.__click(fgui.Events.createEvent(Laya.Event.CLICK, this.displayObject));
        }
        setState(val) {
            if (this._buttonController)
                this._buttonController.selectedPage = val;
            if (this._downEffect == 1) {
                var cnt = this.numChildren;
                if (val == GButton.DOWN || val == GButton.SELECTED_OVER || val == GButton.SELECTED_DISABLED) {
                    var r = this._downEffectValue * 255;
                    var color = Laya.Utils.toHexColor((r << 16) + (r << 8) + r);
                    for (var i = 0; i < cnt; i++) {
                        var obj = this.getChildAt(i);
                        if (!(obj instanceof fgui.GTextField))
                            obj.setProp(fgui.ObjectPropID.Color, color);
                    }
                }
                else {
                    for (i = 0; i < cnt; i++) {
                        obj = this.getChildAt(i);
                        if (!(obj instanceof fgui.GTextField))
                            obj.setProp(fgui.ObjectPropID.Color, "#FFFFFF");
                    }
                }
            }
            else if (this._downEffect == 2) {
                if (val == GButton.DOWN || val == GButton.SELECTED_OVER || val == GButton.SELECTED_DISABLED) {
                    if (!this._downScaled) {
                        this.setScale(this.scaleX * this._downEffectValue, this.scaleY * this._downEffectValue);
                        this._downScaled = true;
                    }
                }
                else {
                    if (this._downScaled) {
                        this.setScale(this.scaleX / this._downEffectValue, this.scaleY / this._downEffectValue);
                        this._downScaled = false;
                    }
                }
            }
        }
        handleControllerChanged(c) {
            super.handleControllerChanged(c);
            if (this._relatedController == c)
                this.selected = this._relatedPageId == c.selectedPageId;
        }
        handleGrayedChanged() {
            if (this._buttonController && this._buttonController.hasPage(GButton.DISABLED)) {
                if (this.grayed) {
                    if (this._selected && this._buttonController.hasPage(GButton.SELECTED_DISABLED))
                        this.setState(GButton.SELECTED_DISABLED);
                    else
                        this.setState(GButton.DISABLED);
                }
                else if (this._selected)
                    this.setState(GButton.DOWN);
                else
                    this.setState(GButton.UP);
            }
            else
                super.handleGrayedChanged();
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.titleColor;
                case fgui.ObjectPropID.OutlineColor:
                    {
                        var tf = this.getTextField();
                        if (tf)
                            return tf.strokeColor;
                        else
                            return 0;
                    }
                case fgui.ObjectPropID.FontSize:
                    return this.titleFontSize;
                case fgui.ObjectPropID.Selected:
                    return this.selected;
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.titleColor = value;
                    break;
                case fgui.ObjectPropID.OutlineColor:
                    {
                        var tf = this.getTextField();
                        if (tf)
                            tf.strokeColor = value;
                    }
                    break;
                case fgui.ObjectPropID.FontSize:
                    this.titleFontSize = value;
                    break;
                case fgui.ObjectPropID.Selected:
                    this.selected = value;
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        constructExtension(buffer) {
            buffer.seek(0, 6);
            this._mode = buffer.readByte();
            var str = buffer.readS();
            if (str)
                this._sound = str;
            this._soundVolumeScale = buffer.getFloat32();
            this._downEffect = buffer.readByte();
            this._downEffectValue = buffer.getFloat32();
            if (this._downEffect == 2)
                this.setPivot(0.5, 0.5, this.pivotAsAnchor);
            this._buttonController = this.getController("button");
            this._titleObject = this.getChild("title");
            this._iconObject = this.getChild("icon");
            if (this._titleObject)
                this._title = this._titleObject.text;
            if (this._iconObject)
                this._icon = this._iconObject.icon;
            if (this._mode == fgui.ButtonMode.Common)
                this.setState(GButton.UP);
            this.on(Laya.Event.ROLL_OVER, this, this.__rollover);
            this.on(Laya.Event.ROLL_OUT, this, this.__rollout);
            this.on(Laya.Event.MOUSE_DOWN, this, this.__mousedown);
            this.on(Laya.Event.CLICK, this, this.__click);
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            if (!buffer.seek(beginPos, 6))
                return;
            if (buffer.readByte() != this.packageItem.objectType)
                return;
            var str;
            var iv;
            str = buffer.readS();
            if (str != null)
                this.title = str;
            str = buffer.readS();
            if (str != null)
                this.selectedTitle = str;
            str = buffer.readS();
            if (str != null)
                this.icon = str;
            str = buffer.readS();
            if (str != null)
                this.selectedIcon = str;
            if (buffer.readBool())
                this.titleColor = buffer.readColorS();
            iv = buffer.getInt32();
            if (iv != 0)
                this.titleFontSize = iv;
            iv = buffer.getInt16();
            if (iv >= 0)
                this._relatedController = this.parent.getControllerAt(iv);
            this._relatedPageId = buffer.readS();
            str = buffer.readS();
            if (str != null)
                this._sound = str;
            if (buffer.readBool())
                this._soundVolumeScale = buffer.getFloat32();
            this.selected = buffer.readBool();
        }
        __rollover() {
            if (!this._buttonController || !this._buttonController.hasPage(GButton.OVER))
                return;
            this._over = true;
            if (this._down)
                return;
            if (this.grayed && this._buttonController.hasPage(GButton.DISABLED))
                return;
            this.setState(this._selected ? GButton.SELECTED_OVER : GButton.OVER);
        }
        __rollout() {
            if (!this._buttonController || !this._buttonController.hasPage(GButton.OVER))
                return;
            this._over = false;
            if (this._down)
                return;
            if (this.grayed && this._buttonController.hasPage(GButton.DISABLED))
                return;
            this.setState(this._selected ? GButton.DOWN : GButton.UP);
        }
        __mousedown(evt) {
            this._down = true;
            fgui.GRoot.inst.checkPopups(evt.target);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__mouseup);
            if (this._mode == fgui.ButtonMode.Common) {
                if (this.grayed && this._buttonController && this._buttonController.hasPage(GButton.DISABLED))
                    this.setState(GButton.SELECTED_DISABLED);
                else
                    this.setState(GButton.DOWN);
            }
            if (this._linkedPopup) {
                if (this._linkedPopup instanceof fgui.Window)
                    this._linkedPopup.toggleStatus();
                else
                    this.root.togglePopup(this._linkedPopup, this);
            }
        }
        __mouseup() {
            if (this._down) {
                Laya.stage.off(Laya.Event.MOUSE_UP, this, this.__mouseup);
                this._down = false;
                if (this._displayObject == null)
                    return;
                if (this._mode == fgui.ButtonMode.Common) {
                    if (this.grayed && this._buttonController && this._buttonController.hasPage(GButton.DISABLED))
                        this.setState(GButton.DISABLED);
                    else if (this._over)
                        this.setState(GButton.OVER);
                    else
                        this.setState(GButton.UP);
                }
            }
        }
        __click(evt) {
            if (this._sound) {
                var pi = fgui.UIPackage.getItemByURL(this._sound);
                if (pi)
                    fgui.GRoot.inst.playOneShotSound(pi.file);
                else
                    fgui.GRoot.inst.playOneShotSound(this._sound);
            }
            if (this._mode == fgui.ButtonMode.Check) {
                if (this._changeStateOnClick) {
                    this.selected = !this._selected;
                    fgui.Events.dispatch(fgui.Events.STATE_CHANGED, this.displayObject, evt);
                }
            }
            else if (this._mode == fgui.ButtonMode.Radio) {
                if (this._changeStateOnClick && !this._selected) {
                    this.selected = true;
                    fgui.Events.dispatch(fgui.Events.STATE_CHANGED, this.displayObject, evt);
                }
            }
            else {
                if (this._relatedController)
                    this._relatedController.selectedPageId = this._relatedPageId;
            }
        }
    }
    GButton.UP = "up";
    GButton.DOWN = "down";
    GButton.OVER = "over";
    GButton.SELECTED_OVER = "selectedOver";
    GButton.DISABLED = "disabled";
    GButton.SELECTED_DISABLED = "selectedDisabled";
    fgui.GButton = GButton;
})(fgui);

(function (fgui) {
    class GComboBox extends fgui.GComponent {
        constructor() {
            super();
            this._visibleItemCount = fgui.UIConfig.defaultComboBoxVisibleItemCount;
            this._itemsUpdated = true;
            this._selectedIndex = -1;
            this._popupDirection = 0;
            this._items = [];
            this._values = [];
        }
        get text() {
            if (this._titleObject)
                return this._titleObject.text;
            else
                return null;
        }
        set text(value) {
            if (this._titleObject)
                this._titleObject.text = value;
            this.updateGear(6);
        }
        get titleColor() {
            var tf = this.getTextField();
            if (tf)
                return tf.color;
            else
                return "#000000";
        }
        set titleColor(value) {
            var tf = this.getTextField();
            if (tf)
                tf.color = value;
            this.updateGear(4);
        }
        get titleFontSize() {
            var tf = this.getTextField();
            if (tf)
                return tf.fontSize;
            else
                return 0;
        }
        set titleFontSize(value) {
            var tf = this.getTextField();
            if (tf)
                tf.fontSize = value;
        }
        get icon() {
            if (this._iconObject)
                return this._iconObject.icon;
            else
                return null;
        }
        set icon(value) {
            if (this._iconObject)
                this._iconObject.icon = value;
            this.updateGear(7);
        }
        get visibleItemCount() {
            return this._visibleItemCount;
        }
        set visibleItemCount(value) {
            this._visibleItemCount = value;
        }
        get popupDirection() {
            return this._popupDirection;
        }
        set popupDirection(value) {
            this._popupDirection = value;
        }
        get items() {
            return this._items;
        }
        set items(value) {
            if (!value)
                this._items.length = 0;
            else
                this._items = value.concat();
            if (this._items.length > 0) {
                if (this._selectedIndex >= this._items.length)
                    this._selectedIndex = this._items.length - 1;
                else if (this._selectedIndex == -1)
                    this._selectedIndex = 0;
                this.text = this._items[this._selectedIndex];
                if (this._icons && this._selectedIndex < this._icons.length)
                    this.icon = this._icons[this._selectedIndex];
            }
            else {
                this.text = "";
                if (this._icons)
                    this.icon = null;
                this._selectedIndex = -1;
            }
            this._itemsUpdated = true;
        }
        get icons() {
            return this._icons;
        }
        set icons(value) {
            this._icons = value;
            if (this._icons && this._selectedIndex != -1 && this._selectedIndex < this._icons.length)
                this.icon = this._icons[this._selectedIndex];
        }
        get values() {
            return this._values;
        }
        set values(value) {
            if (!value)
                this._values.length = 0;
            else
                this._values = value.concat();
        }
        get selectedIndex() {
            return this._selectedIndex;
        }
        set selectedIndex(val) {
            if (this._selectedIndex == val)
                return;
            this._selectedIndex = val;
            if (this._selectedIndex >= 0 && this._selectedIndex < this._items.length) {
                this.text = this._items[this._selectedIndex];
                if (this._icons && this._selectedIndex < this._icons.length)
                    this.icon = this._icons[this._selectedIndex];
            }
            else {
                this.text = "";
                if (this._icons)
                    this.icon = null;
            }
            this.updateSelectionController();
        }
        get value() {
            return this._values[this._selectedIndex];
        }
        set value(val) {
            var index = this._values.indexOf(val);
            if (index == -1 && val == null)
                index = this._values.indexOf("");
            this.selectedIndex = index;
        }
        getTextField() {
            if (this._titleObject instanceof fgui.GTextField)
                return this._titleObject;
            else if ((this._titleObject instanceof fgui.GLabel) || (this._titleObject instanceof fgui.GButton))
                return this._titleObject.getTextField();
            else
                return null;
        }
        setState(val) {
            if (this._buttonController)
                this._buttonController.selectedPage = val;
        }
        get selectionController() {
            return this._selectionController;
        }
        set selectionController(value) {
            this._selectionController = value;
        }
        handleControllerChanged(c) {
            super.handleControllerChanged(c);
            if (this._selectionController == c)
                this.selectedIndex = c.selectedIndex;
        }
        updateSelectionController() {
            if (this._selectionController && !this._selectionController.changing
                && this._selectedIndex < this._selectionController.pageCount) {
                var c = this._selectionController;
                this._selectionController = null;
                c.selectedIndex = this._selectedIndex;
                this._selectionController = c;
            }
        }
        dispose() {
            if (this.dropdown) {
                this.dropdown.dispose();
                this.dropdown = null;
            }
            this._selectionController = null;
            super.dispose();
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.titleColor;
                case fgui.ObjectPropID.OutlineColor:
                    {
                        var tf = this.getTextField();
                        if (tf)
                            return tf.strokeColor;
                        else
                            return 0;
                    }
                case fgui.ObjectPropID.FontSize:
                    {
                        tf = this.getTextField();
                        if (tf)
                            return tf.fontSize;
                        else
                            return 0;
                    }
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.titleColor = value;
                    break;
                case fgui.ObjectPropID.OutlineColor:
                    {
                        var tf = this.getTextField();
                        if (tf)
                            tf.strokeColor = value;
                    }
                    break;
                case fgui.ObjectPropID.FontSize:
                    {
                        tf = this.getTextField();
                        if (tf)
                            tf.fontSize = value;
                    }
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        constructExtension(buffer) {
            var str;
            this._buttonController = this.getController("button");
            this._titleObject = this.getChild("title");
            this._iconObject = this.getChild("icon");
            str = buffer.readS();
            if (str) {
                this.dropdown = (fgui.UIPackage.createObjectFromURL(str));
                if (!this.dropdown) {
                    Laya.Log.print("下拉框必须为元件");
                    return;
                }
                this.dropdown.name = "this._dropdownObject";
                this._list = this.dropdown.getChild("list");
                if (!this._list) {
                    Laya.Log.print(this.resourceURL + ": 下拉框的弹出元件里必须包含名为list的列表");
                    return;
                }
                this._list.on(fgui.Events.CLICK_ITEM, this, this.__clickItem);
                this._list.addRelation(this.dropdown, fgui.RelationType.Width);
                this._list.removeRelation(this.dropdown, fgui.RelationType.Height);
                this.dropdown.addRelation(this._list, fgui.RelationType.Height);
                this.dropdown.removeRelation(this._list, fgui.RelationType.Width);
                this.dropdown.displayObject.on(Laya.Event.UNDISPLAY, this, this.__popupWinClosed);
            }
            this.on(Laya.Event.ROLL_OVER, this, this.__rollover);
            this.on(Laya.Event.ROLL_OUT, this, this.__rollout);
            this.on(Laya.Event.MOUSE_DOWN, this, this.__mousedown);
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            if (!buffer.seek(beginPos, 6))
                return;
            if (buffer.readByte() != this.packageItem.objectType)
                return;
            var i;
            var iv;
            var nextPos;
            var str;
            var itemCount = buffer.getInt16();
            for (i = 0; i < itemCount; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                this._items[i] = buffer.readS();
                this._values[i] = buffer.readS();
                str = buffer.readS();
                if (str != null) {
                    if (!this._icons)
                        this._icons = [];
                    this._icons[i] = str;
                }
                buffer.pos = nextPos;
            }
            str = buffer.readS();
            if (str != null) {
                this.text = str;
                this._selectedIndex = this._items.indexOf(str);
            }
            else if (this._items.length > 0) {
                this._selectedIndex = 0;
                this.text = this._items[0];
            }
            else
                this._selectedIndex = -1;
            str = buffer.readS();
            if (str != null)
                this.icon = str;
            if (buffer.readBool())
                this.titleColor = buffer.readColorS();
            iv = buffer.getInt32();
            if (iv > 0)
                this._visibleItemCount = iv;
            this._popupDirection = buffer.readByte();
            iv = buffer.getInt16();
            if (iv >= 0)
                this._selectionController = this.parent.getControllerAt(iv);
        }
        showDropdown() {
            if (this._itemsUpdated) {
                this._itemsUpdated = false;
                this._list.removeChildrenToPool();
                var cnt = this._items.length;
                for (var i = 0; i < cnt; i++) {
                    var item = this._list.addItemFromPool();
                    item.name = i < this._values.length ? this._values[i] : "";
                    item.text = this._items[i];
                    item.icon = (this._icons && i < this._icons.length) ? this._icons[i] : null;
                }
                this._list.resizeToFit(this._visibleItemCount);
            }
            this._list.selectedIndex = -1;
            this.dropdown.width = this.width;
            this._list.ensureBoundsCorrect();
            var downward = null;
            if (this._popupDirection == fgui.PopupDirection.Down)
                downward = true;
            else if (this._popupDirection == fgui.PopupDirection.Up)
                downward = false;
            this.root.togglePopup(this.dropdown, this, downward);
            if (this.dropdown.parent)
                this.setState(fgui.GButton.DOWN);
        }
        __popupWinClosed() {
            if (this._over)
                this.setState(fgui.GButton.OVER);
            else
                this.setState(fgui.GButton.UP);
        }
        __clickItem(itemObject, evt) {
            Laya.timer.callLater(this, this.__clickItem2, [this._list.getChildIndex(itemObject), evt]);
        }
        __clickItem2(index, evt) {
            if (this.dropdown.parent instanceof fgui.GRoot)
                this.dropdown.parent.hidePopup();
            this._selectedIndex = -1;
            this.selectedIndex = index;
            fgui.Events.dispatch(fgui.Events.STATE_CHANGED, this.displayObject, evt);
        }
        __rollover() {
            this._over = true;
            if (this._down || this.dropdown && this.dropdown.parent)
                return;
            this.setState(fgui.GButton.OVER);
        }
        __rollout() {
            this._over = false;
            if (this._down || this.dropdown && this.dropdown.parent)
                return;
            this.setState(fgui.GButton.UP);
        }
        __mousedown(evt) {
            if (evt.target instanceof Laya.Input)
                return;
            this._down = true;
            fgui.GRoot.inst.checkPopups(evt.target);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__mouseup);
            if (this.dropdown)
                this.showDropdown();
        }
        __mouseup() {
            if (this._down) {
                this._down = false;
                Laya.stage.off(Laya.Event.MOUSE_UP, this, this.__mouseup);
                if (this.dropdown && !this.dropdown.parent) {
                    if (this._over)
                        this.setState(fgui.GButton.OVER);
                    else
                        this.setState(fgui.GButton.UP);
                }
            }
        }
    }
    fgui.GComboBox = GComboBox;
})(fgui);

(function (fgui) {
    let GGGraphType;
    (function (GGGraphType) {
        GGGraphType[GGGraphType["Empty"] = 0] = "Empty";
        GGGraphType[GGGraphType["Rect"] = 1] = "Rect";
        GGGraphType[GGGraphType["Ellipse"] = 2] = "Ellipse";
        GGGraphType[GGGraphType["Polygon"] = 3] = "Polygon";
        GGGraphType[GGGraphType["RegularPolygon"] = 4] = "RegularPolygon";
    })(GGGraphType = fgui.GGGraphType || (fgui.GGGraphType = {}));
    class GGraph extends fgui.GObject {
        constructor() {
            super();
            this._type = GGGraphType.Empty;
            this._lineSize = 1;
            this._lineColor = "#000000";
            this._fillColor = "#FFFFFF";
        }
        get type() { return this._type; }
        get polygonPoints() { return this._polygonPoints; }
        get fillColor() { return this._fillColor; }
        set fillColor(v) {
            if (v === this._fillColor)
                return;
            this._fillColor = v;
            this.updateGraph();
        }
        get lineColor() {
            return this._lineColor;
        }
        set lineColor(v) {
            if (v === this._fillColor)
                return;
            this._lineColor = v;
            this.updateGraph();
        }
        drawRect(lineSize, lineColor, fillColor, cornerRadius) {
            this._type = GGGraphType.Rect;
            this._lineSize = lineSize;
            this._lineColor = lineColor;
            this._fillColor = fillColor;
            this._cornerRadius = cornerRadius;
            this.updateGraph();
        }
        drawEllipse(lineSize, lineColor, fillColor) {
            this._type = GGGraphType.Ellipse;
            this._lineSize = lineSize;
            this._lineColor = lineColor;
            this._fillColor = fillColor;
            this.updateGraph();
        }
        drawRegularPolygon(lineSize, lineColor, fillColor, sides, startAngle, distances) {
            this._type = GGGraphType.RegularPolygon;
            this._lineSize = lineSize;
            this._lineColor = lineColor;
            this._fillColor = fillColor;
            this._sides = sides;
            this._startAngle = startAngle || 0;
            this._distances = distances;
            this.updateGraph();
        }
        drawPolygon(lineSize, lineColor, fillColor, points) {
            this._type = GGGraphType.Polygon;
            this._lineSize = lineSize;
            this._lineColor = lineColor;
            this._fillColor = fillColor;
            this._polygonPoints = points;
            this.updateGraph();
        }
        get distances() {
            return this._distances;
        }
        set distances(value) {
            this._distances = value;
            if (this._type == 3)
                this.updateGraph();
        }
        get color() {
            return this._fillColor;
        }
        set color(value) {
            this._fillColor = value;
            this.updateGear(4);
            if (this._type != 0)
                this.updateGraph();
        }
        updateGraph() {
            this._displayObject.mouseEnabled = this.touchable;
            var gr = this._displayObject.graphics;
            gr.clear();
            var w = this.width;
            var h = this.height;
            if (w == 0 || h == 0)
                return;
            var fillColor = this._fillColor;
            var lineColor = this._lineColor;
            if ( /*Render.isWebGL &&*/fgui.ToolSet.startsWith(fillColor, "rgba")) {
                //webgl下laya未支持rgba格式
                var arr = fillColor.substring(5, fillColor.lastIndexOf(")")).split(",");
                var a = parseFloat(arr[3]);
                if (a == 0)
                    fillColor = null;
                else {
                    fillColor = Laya.Utils.toHexColor((parseInt(arr[0]) << 16) + (parseInt(arr[1]) << 8) + parseInt(arr[2]));
                    this.alpha = a;
                }
            }
            if (this._type == 1) {
                if (this._cornerRadius) {
                    var paths = [
                        ["moveTo", this._cornerRadius[0], 0],
                        ["lineTo", w - this._cornerRadius[1], 0],
                        ["arcTo", w, 0, w, this._cornerRadius[1], this._cornerRadius[1]],
                        ["lineTo", w, h - this._cornerRadius[3]],
                        ["arcTo", w, h, w - this._cornerRadius[3], h, this._cornerRadius[3]],
                        ["lineTo", this._cornerRadius[2], h],
                        ["arcTo", 0, h, 0, h - this._cornerRadius[2], this._cornerRadius[2]],
                        ["lineTo", 0, this._cornerRadius[0]],
                        ["arcTo", 0, 0, this._cornerRadius[0], 0, this._cornerRadius[0]],
                        ["closePath"]
                    ];
                    gr.drawPath(0, 0, paths, fillColor ? { fillStyle: fillColor } : null, this._lineSize > 0 ? { strokeStyle: lineColor, lineWidth: this._lineSize } : null);
                }
                else
                    gr.drawRect(0, 0, w, h, fillColor, this._lineSize > 0 ? lineColor : null, this._lineSize);
            }
            else if (this._type == 2) {
                gr.drawCircle(w / 2, h / 2, w / 2, fillColor, this._lineSize > 0 ? lineColor : null, this._lineSize);
            }
            else if (this._type == 3) {
                gr.drawPoly(0, 0, this._polygonPoints, fillColor, this._lineSize > 0 ? lineColor : null, this._lineSize);
            }
            else if (this._type == 4) {
                if (!this._polygonPoints)
                    this._polygonPoints = [];
                var radius = Math.min(this._width, this._height) / 2;
                this._polygonPoints.length = 0;
                var angle = Laya.Utils.toRadian(this._startAngle);
                var deltaAngle = 2 * Math.PI / this._sides;
                var dist;
                for (var i = 0; i < this._sides; i++) {
                    if (this._distances) {
                        dist = this._distances[i];
                        if (isNaN(dist))
                            dist = 1;
                    }
                    else
                        dist = 1;
                    var xv = radius + radius * dist * Math.cos(angle);
                    var yv = radius + radius * dist * Math.sin(angle);
                    this._polygonPoints.push(xv, yv);
                    angle += deltaAngle;
                }
                gr.drawPoly(0, 0, this._polygonPoints, fillColor, this._lineSize > 0 ? lineColor : null, this._lineSize);
            }
            this._displayObject.repaint();
        }
        replaceMe(target) {
            if (!this._parent)
                throw "parent not set";
            target.name = this.name;
            target.alpha = this.alpha;
            target.rotation = this.rotation;
            target.visible = this.visible;
            target.touchable = this.touchable;
            target.grayed = this.grayed;
            target.setXY(this.x, this.y);
            target.setSize(this.width, this.height);
            var index = this._parent.getChildIndex(this);
            this._parent.addChildAt(target, index);
            target.relations.copyFrom(this.relations);
            this._parent.removeChild(this, true);
        }
        addBeforeMe(target) {
            if (!this._parent)
                throw "parent not set";
            var index = this._parent.getChildIndex(this);
            this._parent.addChildAt(target, index);
        }
        addAfterMe(target) {
            if (!this._parent)
                throw "parent not set";
            var index = this._parent.getChildIndex(this);
            index++;
            this._parent.addChildAt(target, index);
        }
        setNativeObject(obj) {
            this._type = 0;
            this._displayObject.mouseEnabled = this.touchable;
            this._displayObject.graphics.clear();
            this._displayObject.addChild(obj);
        }
        createDisplayObject() {
            super.createDisplayObject();
            this._displayObject.mouseEnabled = false;
            this._hitArea = new Laya.HitArea();
            this._hitArea.hit = this._displayObject.graphics;
            this._displayObject.hitArea = this._hitArea;
        }
        getProp(index) {
            if (index == fgui.ObjectPropID.Color)
                return this.color;
            else
                return super.getProp(index);
        }
        setProp(index, value) {
            if (index == fgui.ObjectPropID.Color)
                this.color = value;
            else
                super.setProp(index, value);
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            if (this._type != 0)
                this.updateGraph();
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            this._type = buffer.readByte();
            if (this._type != 0) {
                var i;
                var cnt;
                this._lineSize = buffer.getInt32();
                this._lineColor = buffer.readColorS(true);
                this._fillColor = buffer.readColorS(true);
                if (buffer.readBool()) {
                    this._cornerRadius = [];
                    for (i = 0; i < 4; i++)
                        this._cornerRadius[i] = buffer.getFloat32();
                }
                if (this._type == 3) {
                    cnt = buffer.getInt16();
                    this._polygonPoints = [];
                    this._polygonPoints.length = cnt;
                    for (i = 0; i < cnt; i++)
                        this._polygonPoints[i] = buffer.getFloat32();
                }
                else if (this._type == 4) {
                    this._sides = buffer.getInt16();
                    this._startAngle = buffer.getFloat32();
                    cnt = buffer.getInt16();
                    if (cnt > 0) {
                        this._distances = [];
                        for (i = 0; i < cnt; i++)
                            this._distances[i] = buffer.getFloat32();
                    }
                }
                this.updateGraph();
            }
        }
    }
    fgui.GGraph = GGraph;
})(fgui);

(function (fgui) {
    class GGroup extends fgui.GObject {
        constructor() {
            super();
            this._layout = 0;
            this._lineGap = 0;
            this._columnGap = 0;
            this._mainGridIndex = -1;
            this._mainGridMinSize = 50;
            this._mainChildIndex = -1;
            this._totalSize = 0;
            this._numChildren = 0;
            this._updating = 0;
        }
        dispose() {
            this._boundsChanged = false;
            super.dispose();
        }
        get layout() {
            return this._layout;
        }
        set layout(value) {
            if (this._layout != value) {
                this._layout = value;
                this.setBoundsChangedFlag();
            }
        }
        get lineGap() {
            return this._lineGap;
        }
        set lineGap(value) {
            if (this._lineGap != value) {
                this._lineGap = value;
                this.setBoundsChangedFlag(true);
            }
        }
        get columnGap() {
            return this._columnGap;
        }
        set columnGap(value) {
            if (this._columnGap != value) {
                this._columnGap = value;
                this.setBoundsChangedFlag(true);
            }
        }
        get excludeInvisibles() {
            return this._excludeInvisibles;
        }
        set excludeInvisibles(value) {
            if (this._excludeInvisibles != value) {
                this._excludeInvisibles = value;
                this.setBoundsChangedFlag();
            }
        }
        get autoSizeDisabled() {
            return this._autoSizeDisabled;
        }
        set autoSizeDisabled(value) {
            this._autoSizeDisabled = value;
        }
        get mainGridMinSize() {
            return this._mainGridMinSize;
        }
        set mainGridMinSize(value) {
            if (this._mainGridMinSize != value) {
                this._mainGridMinSize = value;
                this.setBoundsChangedFlag();
            }
        }
        get mainGridIndex() {
            return this._mainGridIndex;
        }
        set mainGridIndex(value) {
            if (this._mainGridIndex != value) {
                this._mainGridIndex = value;
                this.setBoundsChangedFlag();
            }
        }
        setBoundsChangedFlag(positionChangedOnly) {
            if (this._updating == 0 && this._parent) {
                if (!positionChangedOnly)
                    this._percentReady = false;
                if (!this._boundsChanged) {
                    this._boundsChanged = true;
                    if (this._layout != fgui.GroupLayoutType.None)
                        Laya.timer.callLater(this, this.ensureBoundsCorrect);
                }
            }
        }
        ensureSizeCorrect() {
            if (!this._parent || !this._boundsChanged || this._layout == 0)
                return;
            this._boundsChanged = false;
            if (this._autoSizeDisabled)
                this.resizeChildren(0, 0);
            else {
                this.handleLayout();
                this.updateBounds();
            }
        }
        ensureBoundsCorrect() {
            if (!this._parent || !this._boundsChanged)
                return;
            this._boundsChanged = false;
            if (this._layout == 0)
                this.updateBounds();
            else {
                if (this._autoSizeDisabled)
                    this.resizeChildren(0, 0);
                else {
                    this.handleLayout();
                    this.updateBounds();
                }
            }
        }
        updateBounds() {
            Laya.timer.clear(this, this.ensureBoundsCorrect);
            var cnt = this._parent.numChildren;
            var i;
            var child;
            var ax = Number.POSITIVE_INFINITY, ay = Number.POSITIVE_INFINITY;
            var ar = Number.NEGATIVE_INFINITY, ab = Number.NEGATIVE_INFINITY;
            var tmp;
            var empty = true;
            for (i = 0; i < cnt; i++) {
                child = this._parent.getChildAt(i);
                if (child.group != this || this._excludeInvisibles && !child.internalVisible3)
                    continue;
                tmp = child.xMin;
                if (tmp < ax)
                    ax = tmp;
                tmp = child.yMin;
                if (tmp < ay)
                    ay = tmp;
                tmp = child.xMin + child.width;
                if (tmp > ar)
                    ar = tmp;
                tmp = child.yMin + child.height;
                if (tmp > ab)
                    ab = tmp;
                empty = false;
            }
            var w = 0, h = 0;
            if (!empty) {
                this._updating |= 1;
                this.setXY(ax, ay);
                this._updating &= 2;
                w = ar - ax;
                h = ab - ay;
            }
            if ((this._updating & 2) == 0) {
                this._updating |= 2;
                this.setSize(w, h);
                this._updating &= 1;
            }
            else {
                this._updating &= 1;
                this.resizeChildren(this._width - w, this._height - h);
            }
        }
        handleLayout() {
            this._updating |= 1;
            var child;
            var i;
            var cnt;
            if (this._layout == fgui.GroupLayoutType.Horizontal) {
                var curX = this.x;
                cnt = this._parent.numChildren;
                for (i = 0; i < cnt; i++) {
                    child = this._parent.getChildAt(i);
                    if (child.group != this)
                        continue;
                    if (this._excludeInvisibles && !child.internalVisible3)
                        continue;
                    child.xMin = curX;
                    if (child.width != 0)
                        curX += child.width + this._columnGap;
                }
            }
            else if (this._layout == fgui.GroupLayoutType.Vertical) {
                var curY = this.y;
                cnt = this._parent.numChildren;
                for (i = 0; i < cnt; i++) {
                    child = this._parent.getChildAt(i);
                    if (child.group != this)
                        continue;
                    if (this._excludeInvisibles && !child.internalVisible3)
                        continue;
                    child.yMin = curY;
                    if (child.height != 0)
                        curY += child.height + this._lineGap;
                }
            }
            this._updating &= 2;
        }
        moveChildren(dx, dy) {
            if ((this._updating & 1) != 0 || !this._parent)
                return;
            this._updating |= 1;
            var cnt = this._parent.numChildren;
            var i;
            var child;
            for (i = 0; i < cnt; i++) {
                child = this._parent.getChildAt(i);
                if (child.group == this) {
                    child.setXY(child.x + dx, child.y + dy);
                }
            }
            this._updating &= 2;
        }
        resizeChildren(dw, dh) {
            if (this._layout == fgui.GroupLayoutType.None || (this._updating & 2) != 0 || !this._parent)
                return;
            this._updating |= 2;
            if (this._boundsChanged) {
                this._boundsChanged = false;
                if (!this._autoSizeDisabled) {
                    this.updateBounds();
                    return;
                }
            }
            var cnt = this._parent.numChildren;
            var i;
            var child;
            if (!this._percentReady) {
                this._percentReady = true;
                this._numChildren = 0;
                this._totalSize = 0;
                this._mainChildIndex = -1;
                var j = 0;
                for (i = 0; i < cnt; i++) {
                    child = this._parent.getChildAt(i);
                    if (child.group != this)
                        continue;
                    if (!this._excludeInvisibles || child.internalVisible3) {
                        if (j == this._mainGridIndex)
                            this._mainChildIndex = i;
                        this._numChildren++;
                        if (this._layout == 1)
                            this._totalSize += child.width;
                        else
                            this._totalSize += child.height;
                    }
                    j++;
                }
                if (this._mainChildIndex != -1) {
                    if (this._layout == 1) {
                        child = this._parent.getChildAt(this._mainChildIndex);
                        this._totalSize += this._mainGridMinSize - child.width;
                        child._sizePercentInGroup = this._mainGridMinSize / this._totalSize;
                    }
                    else {
                        child = this._parent.getChildAt(this._mainChildIndex);
                        this._totalSize += this._mainGridMinSize - child.height;
                        child._sizePercentInGroup = this._mainGridMinSize / this._totalSize;
                    }
                }
                for (i = 0; i < cnt; i++) {
                    child = this._parent.getChildAt(i);
                    if (child.group != this)
                        continue;
                    if (i == this._mainChildIndex)
                        continue;
                    if (this._totalSize > 0)
                        child._sizePercentInGroup = (this._layout == 1 ? child.width : child.height) / this._totalSize;
                    else
                        child._sizePercentInGroup = 0;
                }
            }
            var remainSize = 0;
            var remainPercent = 1;
            var priorHandled = false;
            if (this._layout == 1) {
                remainSize = this.width - (this._numChildren - 1) * this._columnGap;
                if (this._mainChildIndex != -1 && remainSize >= this._totalSize) {
                    child = this._parent.getChildAt(this._mainChildIndex);
                    child.setSize(remainSize - (this._totalSize - this._mainGridMinSize), child._rawHeight + dh, true);
                    remainSize -= child.width;
                    remainPercent -= child._sizePercentInGroup;
                    priorHandled = true;
                }
                var curX = this.x;
                for (i = 0; i < cnt; i++) {
                    child = this._parent.getChildAt(i);
                    if (child.group != this)
                        continue;
                    if (this._excludeInvisibles && !child.internalVisible3) {
                        child.setSize(child._rawWidth, child._rawHeight + dh, true);
                        continue;
                    }
                    if (!priorHandled || i != this._mainChildIndex) {
                        child.setSize(Math.round(child._sizePercentInGroup / remainPercent * remainSize), child._rawHeight + dh, true);
                        remainPercent -= child._sizePercentInGroup;
                        remainSize -= child.width;
                    }
                    child.xMin = curX;
                    if (child.width != 0)
                        curX += child.width + this._columnGap;
                }
            }
            else {
                remainSize = this.height - (this._numChildren - 1) * this._lineGap;
                if (this._mainChildIndex != -1 && remainSize >= this._totalSize) {
                    child = this._parent.getChildAt(this._mainChildIndex);
                    child.setSize(child._rawWidth + dw, remainSize - (this._totalSize - this._mainGridMinSize), true);
                    remainSize -= child.height;
                    remainPercent -= child._sizePercentInGroup;
                    priorHandled = true;
                }
                var curY = this.y;
                for (i = 0; i < cnt; i++) {
                    child = this._parent.getChildAt(i);
                    if (child.group != this)
                        continue;
                    if (this._excludeInvisibles && !child.internalVisible3) {
                        child.setSize(child._rawWidth + dw, child._rawHeight, true);
                        continue;
                    }
                    if (!priorHandled || i != this._mainChildIndex) {
                        child.setSize(child._rawWidth + dw, Math.round(child._sizePercentInGroup / remainPercent * remainSize), true);
                        remainPercent -= child._sizePercentInGroup;
                        remainSize -= child.height;
                    }
                    child.yMin = curY;
                    if (child.height != 0)
                        curY += child.height + this._lineGap;
                }
            }
            this._updating &= 1;
        }
        handleAlphaChanged() {
            if (this._underConstruct)
                return;
            var cnt = this._parent.numChildren;
            for (var i = 0; i < cnt; i++) {
                var child = this._parent.getChildAt(i);
                if (child.group == this)
                    child.alpha = this.alpha;
            }
        }
        handleVisibleChanged() {
            if (!this._parent)
                return;
            var cnt = this._parent.numChildren;
            for (var i = 0; i < cnt; i++) {
                var child = this._parent.getChildAt(i);
                if (child.group == this)
                    child.handleVisibleChanged();
            }
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            this._layout = buffer.readByte();
            this._lineGap = buffer.getInt32();
            this._columnGap = buffer.getInt32();
            if (buffer.version >= 2) {
                this._excludeInvisibles = buffer.readBool();
                this._autoSizeDisabled = buffer.readBool();
                this._mainGridIndex = buffer.getInt16();
            }
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            if (!this.visible)
                this.handleVisibleChanged();
        }
    }
    fgui.GGroup = GGroup;
})(fgui);

(function (fgui) {
    class GImage extends fgui.GObject {
        constructor() {
            super();
            this._flip = 0;
        }
        get image() {
            return this._image;
        }
        get color() {
            return this.image.color;
        }
        set color(value) {
            if (this.image.color != value) {
                this.image.color = value;
                this.updateGear(4);
            }
        }
        get flip() {
            return this._flip;
        }
        set flip(value) {
            if (this._flip != value) {
                this._flip = value;
                var sx = 1, sy = 1;
                if (this._flip == fgui.FlipType.Horizontal || this._flip == fgui.FlipType.Both)
                    sx = -1;
                if (this._flip == fgui.FlipType.Vertical || this._flip == fgui.FlipType.Both)
                    sy = -1;
                this.setScale(sx, sy);
                this.handleXYChanged();
            }
        }
        get fillMethod() {
            return this.image.fillMethod;
        }
        set fillMethod(value) {
            this.image.fillMethod = value;
        }
        get fillOrigin() {
            return this.image.fillOrigin;
        }
        set fillOrigin(value) {
            this.image.fillOrigin = value;
        }
        get fillClockwise() {
            return this.image.fillClockwise;
        }
        set fillClockwise(value) {
            this.image.fillClockwise = value;
        }
        get fillAmount() {
            return this.image.fillAmount;
        }
        set fillAmount(value) {
            this.image.fillAmount = value;
        }
        createDisplayObject() {
            this._displayObject = this._image = new fgui.Image();
            this.image.mouseEnabled = false;
            this._displayObject["$owner"] = this;
        }
        constructFromResource() {
            this._contentItem = this.packageItem.getBranch();
            this.sourceWidth = this._contentItem.width;
            this.sourceHeight = this._contentItem.height;
            this.initWidth = this.sourceWidth;
            this.initHeight = this.sourceHeight;
            this._contentItem = this._contentItem.getHighResolution();
            this._contentItem.load();
            this.image.scale9Grid = this._contentItem.scale9Grid;
            this.image.scaleByTile = this._contentItem.scaleByTile;
            this.image.tileGridIndice = this._contentItem.tileGridIndice;
            this.image.texture = this._contentItem.texture;
            this.setSize(this.sourceWidth, this.sourceHeight);
        }
        handleXYChanged() {
            super.handleXYChanged();
            if (this._flip != fgui.FlipType.None) {
                if (this.scaleX == -1)
                    this.image.x += this.width;
                if (this.scaleY == -1)
                    this.image.y += this.height;
            }
        }
        getProp(index) {
            if (index == fgui.ObjectPropID.Color)
                return this.color;
            else
                return super.getProp(index);
        }
        setProp(index, value) {
            if (index == fgui.ObjectPropID.Color)
                this.color = value;
            else
                super.setProp(index, value);
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            if (buffer.readBool())
                this.color = buffer.readColorS();
            this.flip = buffer.readByte();
            this.image.fillMethod = buffer.readByte();
            if (this.image.fillMethod != 0) {
                this.image.fillOrigin = buffer.readByte();
                this.image.fillClockwise = buffer.readBool();
                this.image.fillAmount = buffer.getFloat32();
            }
        }
    }
    fgui.GImage = GImage;
})(fgui);

(function (fgui) {
    class GLabel extends fgui.GComponent {
        constructor() {
            super();
        }
        get icon() {
            if (this._iconObject)
                return this._iconObject.icon;
            else
                return null;
        }
        set icon(value) {
            if (this._iconObject)
                this._iconObject.icon = value;
            this.updateGear(7);
        }
        get title() {
            if (this._titleObject)
                return this._titleObject.text;
            else
                return null;
        }
        set title(value) {
            if (this._titleObject)
                this._titleObject.text = value;
            this.updateGear(6);
        }
        get text() {
            return this.title;
        }
        set text(value) {
            this.title = value;
        }
        get titleColor() {
            var tf = this.getTextField();
            if (tf)
                return tf.color;
            else
                return "#000000";
        }
        set titleColor(value) {
            var tf = this.getTextField();
            if (tf)
                tf.color = value;
            this.updateGear(4);
        }
        get titleFontSize() {
            var tf = this.getTextField();
            if (tf)
                return tf.fontSize;
            else
                return 0;
        }
        set titleFontSize(value) {
            var tf = this.getTextField();
            if (tf)
                tf.fontSize = value;
        }
        get color() {
            return this.titleColor;
        }
        set color(value) {
            this.titleColor = value;
        }
        set editable(val) {
            if (this._titleObject)
                this._titleObject.asTextInput.editable = val;
        }
        get editable() {
            if (this._titleObject instanceof fgui.GTextInput)
                return this._titleObject.asTextInput.editable;
            else
                return false;
        }
        getTextField() {
            if (this._titleObject instanceof fgui.GTextField)
                return this._titleObject;
            else if ((this._titleObject instanceof GLabel) || (this._titleObject instanceof fgui.GButton))
                return this._titleObject.getTextField();
            else
                return null;
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.titleColor;
                case fgui.ObjectPropID.OutlineColor:
                    {
                        var tf = this.getTextField();
                        if (tf)
                            return tf.strokeColor;
                        else
                            return 0;
                    }
                case fgui.ObjectPropID.FontSize:
                    return this.titleFontSize;
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.titleColor = value;
                    break;
                case fgui.ObjectPropID.OutlineColor:
                    {
                        var tf = this.getTextField();
                        if (tf)
                            tf.strokeColor = value;
                    }
                    break;
                case fgui.ObjectPropID.FontSize:
                    this.titleFontSize = value;
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        constructExtension(buffer) {
            this._titleObject = this.getChild("title");
            this._iconObject = this.getChild("icon");
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            if (!buffer.seek(beginPos, 6))
                return;
            if (buffer.readByte() != this.packageItem.objectType)
                return;
            var str;
            str = buffer.readS();
            if (str != null)
                this.title = str;
            str = buffer.readS();
            if (str != null)
                this.icon = str;
            if (buffer.readBool())
                this.titleColor = buffer.readColorS();
            var iv = buffer.getInt32();
            if (iv != 0)
                this.titleFontSize = iv;
            if (buffer.readBool()) {
                var input = this.getTextField();
                if (input instanceof fgui.GTextInput) {
                    str = buffer.readS();
                    if (str != null)
                        input.promptText = str;
                    str = buffer.readS();
                    if (str != null)
                        input.restrict = str;
                    iv = buffer.getInt32();
                    if (iv != 0)
                        input.maxLength = iv;
                    iv = buffer.getInt32();
                    if (iv != 0) {
                        if (iv == 4)
                            input.keyboardType = Laya.Input.TYPE_NUMBER;
                        else if (iv == 3)
                            input.keyboardType = Laya.Input.TYPE_URL;
                    }
                    if (buffer.readBool())
                        input.password = true;
                }
                else
                    buffer.skip(13);
            }
        }
    }
    fgui.GLabel = GLabel;
})(fgui);

(function (fgui) {
    class GList extends fgui.GComponent {
        constructor() {
            super();
            this._lineCount = 0;
            this._columnCount = 0;
            this._lineGap = 0;
            this._columnGap = 0;
            this._lastSelectedIndex = 0;
            this._numItems = 0;
            this._firstIndex = 0; //the top left index
            this._curLineItemCount = 0; //item count in one line
            this._virtualListChanged = 0; //1-content changed, 2-size changed
            this.itemInfoVer = 0; //用来标志item是否在本次处理中已经被重用了
            this._trackBounds = true;
            this._pool = new fgui.GObjectPool();
            this._layout = fgui.ListLayoutType.SingleColumn;
            this._autoResizeItem = true;
            this._lastSelectedIndex = -1;
            this._selectionMode = fgui.ListSelectionMode.Single;
            this.opaque = true;
            this.scrollItemToViewOnClick = true;
            this._align = "left";
            this._verticalAlign = "top";
            this._container = new Laya.Sprite();
            this._displayObject.addChild(this._container);
        }
        dispose() {
            this._pool.clear();
            super.dispose();
        }
        get layout() {
            return this._layout;
        }
        set layout(value) {
            if (this._layout != value) {
                this._layout = value;
                this.setBoundsChangedFlag();
                if (this._virtual)
                    this.setVirtualListChangedFlag(true);
            }
        }
        get lineCount() {
            return this._lineCount;
        }
        set lineCount(value) {
            if (this._lineCount != value) {
                this._lineCount = value;
                if (this._layout == fgui.ListLayoutType.FlowVertical || this._layout == fgui.ListLayoutType.Pagination) {
                    this.setBoundsChangedFlag();
                    if (this._virtual)
                        this.setVirtualListChangedFlag(true);
                }
            }
        }
        get columnCount() {
            return this._columnCount;
        }
        set columnCount(value) {
            if (this._columnCount != value) {
                this._columnCount = value;
                if (this._layout == fgui.ListLayoutType.FlowHorizontal || this._layout == fgui.ListLayoutType.Pagination) {
                    this.setBoundsChangedFlag();
                    if (this._virtual)
                        this.setVirtualListChangedFlag(true);
                }
            }
        }
        get lineGap() {
            return this._lineGap;
        }
        set lineGap(value) {
            if (this._lineGap != value) {
                this._lineGap = value;
                this.setBoundsChangedFlag();
                if (this._virtual)
                    this.setVirtualListChangedFlag(true);
            }
        }
        get columnGap() {
            return this._columnGap;
        }
        set columnGap(value) {
            if (this._columnGap != value) {
                this._columnGap = value;
                this.setBoundsChangedFlag();
                if (this._virtual)
                    this.setVirtualListChangedFlag(true);
            }
        }
        get align() {
            return this._align;
        }
        set align(value) {
            if (this._align != value) {
                this._align = value;
                this.setBoundsChangedFlag();
                if (this._virtual)
                    this.setVirtualListChangedFlag(true);
            }
        }
        get verticalAlign() {
            return this._verticalAlign;
        }
        set verticalAlign(value) {
            if (this._verticalAlign != value) {
                this._verticalAlign = value;
                this.setBoundsChangedFlag();
                if (this._virtual)
                    this.setVirtualListChangedFlag(true);
            }
        }
        get virtualItemSize() {
            return this._itemSize;
        }
        set virtualItemSize(value) {
            if (this._virtual) {
                if (this._itemSize == null)
                    this._itemSize = new Laya.Point();
                this._itemSize.setTo(value.x, value.y);
                this.setVirtualListChangedFlag(true);
            }
        }
        get defaultItem() {
            return this._defaultItem;
        }
        set defaultItem(val) {
            this._defaultItem = fgui.UIPackage.normalizeURL(val);
        }
        get autoResizeItem() {
            return this._autoResizeItem;
        }
        set autoResizeItem(value) {
            if (this._autoResizeItem != value) {
                this._autoResizeItem = value;
                this.setBoundsChangedFlag();
                if (this._virtual)
                    this.setVirtualListChangedFlag(true);
            }
        }
        get selectionMode() {
            return this._selectionMode;
        }
        set selectionMode(value) {
            this._selectionMode = value;
        }
        get selectionController() {
            return this._selectionController;
        }
        set selectionController(value) {
            this._selectionController = value;
        }
        get itemPool() {
            return this._pool;
        }
        getFromPool(url) {
            if (!url)
                url = this._defaultItem;
            var obj = this._pool.getObject(url);
            if (obj)
                obj.visible = true;
            return obj;
        }
        returnToPool(obj) {
            obj.displayObject.cacheAs = "none";
            this._pool.returnObject(obj);
        }
        addChildAt(child, index) {
            super.addChildAt(child, index);
            if (child instanceof fgui.GButton) {
                child.selected = false;
                child.changeStateOnClick = false;
            }
            child.on(Laya.Event.CLICK, this, this.__clickItem);
            return child;
        }
        addItem(url) {
            if (!url)
                url = this._defaultItem;
            return this.addChild(fgui.UIPackage.createObjectFromURL(url));
        }
        addItemFromPool(url) {
            return this.addChild(this.getFromPool(url));
        }
        removeChildAt(index, dispose) {
            var child = super.removeChildAt(index);
            if (dispose)
                child.dispose();
            else
                child.off(Laya.Event.CLICK, this, this.__clickItem);
            return child;
        }
        removeChildToPoolAt(index) {
            var child = super.removeChildAt(index);
            this.returnToPool(child);
        }
        removeChildToPool(child) {
            super.removeChild(child);
            this.returnToPool(child);
        }
        removeChildrenToPool(beginIndex, endIndex) {
            if (beginIndex == undefined)
                beginIndex = 0;
            if (endIndex == undefined)
                endIndex = -1;
            if (endIndex < 0 || endIndex >= this._children.length)
                endIndex = this._children.length - 1;
            for (var i = beginIndex; i <= endIndex; ++i)
                this.removeChildToPoolAt(beginIndex);
        }
        get selectedIndex() {
            var i;
            if (this._virtual) {
                for (i = 0; i < this._realNumItems; i++) {
                    var ii = this._virtualItems[i];
                    if ((ii.obj instanceof fgui.GButton) && ii.obj.selected
                        || ii.obj == null && ii.selected) {
                        if (this._loop)
                            return i % this._numItems;
                        else
                            return i;
                    }
                }
            }
            else {
                var cnt = this._children.length;
                for (i = 0; i < cnt; i++) {
                    var obj = this._children[i];
                    if ((obj instanceof fgui.GButton) && obj.selected)
                        return i;
                }
            }
            return -1;
        }
        set selectedIndex(value) {
            if (value >= 0 && value < this.numItems) {
                if (this._selectionMode != fgui.ListSelectionMode.Single)
                    this.clearSelection();
                this.addSelection(value);
            }
            else
                this.clearSelection();
        }
        getSelection(result) {
            if (!result)
                result = new Array();
            var i;
            if (this._virtual) {
                for (i = 0; i < this._realNumItems; i++) {
                    var ii = this._virtualItems[i];
                    if ((ii.obj instanceof fgui.GButton) && ii.obj.selected
                        || ii.obj == null && ii.selected) {
                        var j = i;
                        if (this._loop) {
                            j = i % this._numItems;
                            if (result.indexOf(j) != -1)
                                continue;
                        }
                        result.push(j);
                    }
                }
            }
            else {
                var cnt = this._children.length;
                for (i = 0; i < cnt; i++) {
                    var obj = this._children[i];
                    if ((obj instanceof fgui.GButton) && obj.selected)
                        result.push(i);
                }
            }
            return result;
        }
        addSelection(index, scrollItToView) {
            if (this._selectionMode == fgui.ListSelectionMode.None)
                return;
            this.checkVirtualList();
            if (this._selectionMode == fgui.ListSelectionMode.Single)
                this.clearSelection();
            if (scrollItToView)
                this.scrollToView(index);
            this._lastSelectedIndex = index;
            var obj;
            if (this._virtual) {
                var ii = this._virtualItems[index];
                if (ii.obj)
                    obj = ii.obj;
                ii.selected = true;
            }
            else
                obj = this.getChildAt(index);
            if ((obj instanceof fgui.GButton) && !obj.selected) {
                obj.selected = true;
                this.updateSelectionController(index);
            }
        }
        removeSelection(index) {
            if (this._selectionMode == fgui.ListSelectionMode.None)
                return;
            var obj;
            if (this._virtual) {
                var ii = this._virtualItems[index];
                if (ii.obj)
                    obj = ii.obj;
                ii.selected = false;
            }
            else
                obj = this.getChildAt(index);
            if (obj instanceof fgui.GButton)
                obj.selected = false;
        }
        clearSelection() {
            var i;
            if (this._virtual) {
                for (i = 0; i < this._realNumItems; i++) {
                    var ii = this._virtualItems[i];
                    if (ii.obj instanceof fgui.GButton)
                        ii.obj.selected = false;
                    ii.selected = false;
                }
            }
            else {
                var cnt = this._children.length;
                for (i = 0; i < cnt; i++) {
                    var obj = this._children[i];
                    if (obj instanceof fgui.GButton)
                        obj.selected = false;
                }
            }
        }
        clearSelectionExcept(g) {
            var i;
            if (this._virtual) {
                for (i = 0; i < this._realNumItems; i++) {
                    var ii = this._virtualItems[i];
                    if (ii.obj != g) {
                        if (ii.obj instanceof fgui.GButton)
                            ii.obj.selected = false;
                        ii.selected = false;
                    }
                }
            }
            else {
                var cnt = this._children.length;
                for (i = 0; i < cnt; i++) {
                    var obj = this._children[i];
                    if ((obj instanceof fgui.GButton) && obj != g)
                        obj.selected = false;
                }
            }
        }
        selectAll() {
            this.checkVirtualList();
            var last = -1;
            var i;
            if (this._virtual) {
                for (i = 0; i < this._realNumItems; i++) {
                    var ii = this._virtualItems[i];
                    if ((ii.obj instanceof fgui.GButton) && !ii.obj.selected) {
                        ii.obj.selected = true;
                        last = i;
                    }
                    ii.selected = true;
                }
            }
            else {
                var cnt = this._children.length;
                for (i = 0; i < cnt; i++) {
                    var obj = this._children[i];
                    if ((obj instanceof fgui.GButton) && !obj.selected) {
                        obj.selected = true;
                        last = i;
                    }
                }
            }
            if (last != -1)
                this.updateSelectionController(last);
        }
        selectNone() {
            this.clearSelection();
        }
        selectReverse() {
            this.checkVirtualList();
            var last = -1;
            var i;
            if (this._virtual) {
                for (i = 0; i < this._realNumItems; i++) {
                    var ii = this._virtualItems[i];
                    if (ii.obj instanceof fgui.GButton) {
                        ii.obj.selected = !ii.obj.selected;
                        if (ii.obj.selected)
                            last = i;
                    }
                    ii.selected = !ii.selected;
                }
            }
            else {
                var cnt = this._children.length;
                for (i = 0; i < cnt; i++) {
                    var obj = this._children[i];
                    if (obj instanceof fgui.GButton) {
                        obj.selected = !obj.selected;
                        if (obj.selected)
                            last = i;
                    }
                }
            }
            if (last != -1)
                this.updateSelectionController(last);
        }
        handleArrowKey(dir) {
            var index = this.selectedIndex;
            if (index == -1)
                return;
            switch (dir) {
                case 1: //up
                    if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowVertical) {
                        index--;
                        if (index >= 0) {
                            this.clearSelection();
                            this.addSelection(index, true);
                        }
                    }
                    else if (this._layout == fgui.ListLayoutType.FlowHorizontal || this._layout == fgui.ListLayoutType.Pagination) {
                        var current = this._children[index];
                        var k = 0;
                        for (var i = index - 1; i >= 0; i--) {
                            var obj = this._children[i];
                            if (obj.y != current.y) {
                                current = obj;
                                break;
                            }
                            k++;
                        }
                        for (; i >= 0; i--) {
                            obj = this._children[i];
                            if (obj.y != current.y) {
                                this.clearSelection();
                                this.addSelection(i + k + 1, true);
                                break;
                            }
                        }
                    }
                    break;
                case 3: //right
                    if (this._layout == fgui.ListLayoutType.SingleRow || this._layout == fgui.ListLayoutType.FlowHorizontal || this._layout == fgui.ListLayoutType.Pagination) {
                        index++;
                        if (index < this.numItems) {
                            this.clearSelection();
                            this.addSelection(index, true);
                        }
                    }
                    else if (this._layout == fgui.ListLayoutType.FlowVertical) {
                        current = this._children[index];
                        k = 0;
                        var cnt = this._children.length;
                        for (i = index + 1; i < cnt; i++) {
                            obj = this._children[i];
                            if (obj.x != current.x) {
                                current = obj;
                                break;
                            }
                            k++;
                        }
                        for (; i < cnt; i++) {
                            obj = this._children[i];
                            if (obj.x != current.x) {
                                this.clearSelection();
                                this.addSelection(i - k - 1, true);
                                break;
                            }
                        }
                    }
                    break;
                case 5: //down
                    if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowVertical) {
                        index++;
                        if (index < this.numItems) {
                            this.clearSelection();
                            this.addSelection(index, true);
                        }
                    }
                    else if (this._layout == fgui.ListLayoutType.FlowHorizontal || this._layout == fgui.ListLayoutType.Pagination) {
                        current = this._children[index];
                        k = 0;
                        cnt = this._children.length;
                        for (i = index + 1; i < cnt; i++) {
                            obj = this._children[i];
                            if (obj.y != current.y) {
                                current = obj;
                                break;
                            }
                            k++;
                        }
                        for (; i < cnt; i++) {
                            obj = this._children[i];
                            if (obj.y != current.y) {
                                this.clearSelection();
                                this.addSelection(i - k - 1, true);
                                break;
                            }
                        }
                    }
                    break;
                case 7: //left
                    if (this._layout == fgui.ListLayoutType.SingleRow || this._layout == fgui.ListLayoutType.FlowHorizontal || this._layout == fgui.ListLayoutType.Pagination) {
                        index--;
                        if (index >= 0) {
                            this.clearSelection();
                            this.addSelection(index, true);
                        }
                    }
                    else if (this._layout == fgui.ListLayoutType.FlowVertical) {
                        current = this._children[index];
                        k = 0;
                        for (i = index - 1; i >= 0; i--) {
                            obj = this._children[i];
                            if (obj.x != current.x) {
                                current = obj;
                                break;
                            }
                            k++;
                        }
                        for (; i >= 0; i--) {
                            obj = this._children[i];
                            if (obj.x != current.x) {
                                this.clearSelection();
                                this.addSelection(i + k + 1, true);
                                break;
                            }
                        }
                    }
                    break;
            }
        }
        __clickItem(evt) {
            if (this._scrollPane && this._scrollPane.isDragged)
                return;
            var item = fgui.GObject.cast(evt.currentTarget);
            this.setSelectionOnEvent(item, evt);
            if (this._scrollPane && this.scrollItemToViewOnClick)
                this._scrollPane.scrollToView(item, true);
            this.dispatchItemEvent(item, fgui.Events.createEvent(fgui.Events.CLICK_ITEM, this.displayObject, evt));
        }
        dispatchItemEvent(item, evt) {
            this.displayObject.event(fgui.Events.CLICK_ITEM, [item, evt]);
        }
        setSelectionOnEvent(item, evt) {
            if (!(item instanceof fgui.GButton) || this._selectionMode == fgui.ListSelectionMode.None)
                return;
            var dontChangeLastIndex = false;
            var index = this.childIndexToItemIndex(this.getChildIndex(item));
            if (this._selectionMode == fgui.ListSelectionMode.Single) {
                if (!item.selected) {
                    this.clearSelectionExcept(item);
                    item.selected = true;
                }
            }
            else {
                if (evt.shiftKey) {
                    if (!item.selected) {
                        if (this._lastSelectedIndex != -1) {
                            var min = Math.min(this._lastSelectedIndex, index);
                            var max = Math.max(this._lastSelectedIndex, index);
                            max = Math.min(max, this.numItems - 1);
                            var i;
                            if (this._virtual) {
                                for (i = min; i <= max; i++) {
                                    var ii = this._virtualItems[i];
                                    if (ii.obj instanceof fgui.GButton)
                                        ii.obj.selected = true;
                                    ii.selected = true;
                                }
                            }
                            else {
                                for (i = min; i <= max; i++) {
                                    var obj = this.getChildAt(i);
                                    if (obj instanceof fgui.GButton)
                                        obj.selected = true;
                                }
                            }
                            dontChangeLastIndex = true;
                        }
                        else {
                            item.selected = true;
                        }
                    }
                }
                else if (evt.ctrlKey || this._selectionMode == fgui.ListSelectionMode.Multiple_SingleClick) {
                    item.selected = !item.selected;
                }
                else {
                    if (!item.selected) {
                        this.clearSelectionExcept(item);
                        item.selected = true;
                    }
                    else
                        this.clearSelectionExcept(item);
                }
            }
            if (!dontChangeLastIndex)
                this._lastSelectedIndex = index;
            if (item.selected)
                this.updateSelectionController(index);
        }
        resizeToFit(itemCount, minSize) {
            if (itemCount == null)
                itemCount = 100000;
            minSize = minSize || 0;
            this.ensureBoundsCorrect();
            var curCount = this.numItems;
            if (itemCount > curCount)
                itemCount = curCount;
            if (this._virtual) {
                var lineCount = Math.ceil(itemCount / this._curLineItemCount);
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal)
                    this.viewHeight = lineCount * this._itemSize.y + Math.max(0, lineCount - 1) * this._lineGap;
                else
                    this.viewWidth = lineCount * this._itemSize.x + Math.max(0, lineCount - 1) * this._columnGap;
            }
            else if (itemCount == 0) {
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal)
                    this.viewHeight = minSize;
                else
                    this.viewWidth = minSize;
            }
            else {
                var i = itemCount - 1;
                var obj = null;
                while (i >= 0) {
                    obj = this.getChildAt(i);
                    if (!this.foldInvisibleItems || obj.visible)
                        break;
                    i--;
                }
                if (i < 0) {
                    if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal)
                        this.viewHeight = minSize;
                    else
                        this.viewWidth = minSize;
                }
                else {
                    var size = 0;
                    if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal) {
                        size = obj.y + obj.height;
                        if (size < minSize)
                            size = minSize;
                        this.viewHeight = size;
                    }
                    else {
                        size = obj.x + obj.width;
                        if (size < minSize)
                            size = minSize;
                        this.viewWidth = size;
                    }
                }
            }
        }
        getMaxItemWidth() {
            var cnt = this._children.length;
            var max = 0;
            for (var i = 0; i < cnt; i++) {
                var child = this.getChildAt(i);
                if (child.width > max)
                    max = child.width;
            }
            return max;
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            this.setBoundsChangedFlag();
            if (this._virtual)
                this.setVirtualListChangedFlag(true);
        }
        handleControllerChanged(c) {
            super.handleControllerChanged(c);
            if (this._selectionController == c)
                this.selectedIndex = c.selectedIndex;
        }
        updateSelectionController(index) {
            if (this._selectionController && !this._selectionController.changing
                && index < this._selectionController.pageCount) {
                var c = this._selectionController;
                this._selectionController = null;
                c.selectedIndex = index;
                this._selectionController = c;
            }
        }
        shouldSnapToNext(dir, delta, size) {
            return dir < 0 && delta > fgui.UIConfig.defaultScrollSnappingThreshold * size
                || dir > 0 && delta > (1 - fgui.UIConfig.defaultScrollSnappingThreshold) * size
                || dir == 0 && delta > size / 2;
        }
        getSnappingPositionWithDir(xValue, yValue, xDir, yDir, result) {
            if (this._virtual) {
                if (!result)
                    result = new Laya.Point();
                var saved;
                var index;
                var size;
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal) {
                    saved = yValue;
                    s_n = yValue;
                    index = this.getIndexOnPos1(false);
                    yValue = s_n;
                    if (index < this._virtualItems.length && index < this._realNumItems) {
                        size = this._virtualItems[index].height;
                        if (this.shouldSnapToNext(yDir, saved - yValue, size))
                            yValue += size + this._lineGap;
                    }
                }
                else if (this._layout == fgui.ListLayoutType.SingleRow || this._layout == fgui.ListLayoutType.FlowVertical) {
                    saved = xValue;
                    s_n = xValue;
                    index = this.getIndexOnPos2(false);
                    xValue = s_n;
                    if (index < this._virtualItems.length && index < this._realNumItems) {
                        size = this._virtualItems[index].width;
                        if (this.shouldSnapToNext(xDir, saved - xValue, size))
                            xValue += size + this._columnGap;
                    }
                }
                else {
                    saved = xValue;
                    s_n = xValue;
                    index = this.getIndexOnPos3(false);
                    xValue = s_n;
                    if (index < this._virtualItems.length && index < this._realNumItems) {
                        size = this._virtualItems[index].width;
                        if (this.shouldSnapToNext(xDir, saved - xValue, size))
                            xValue += size + this._columnGap;
                    }
                }
                result.x = xValue;
                result.y = yValue;
                return result;
            }
            else
                return super.getSnappingPositionWithDir(xValue, yValue, xDir, yDir, result);
        }
        scrollToView(index, ani, setFirst) {
            if (this._virtual) {
                if (this._numItems == 0)
                    return;
                this.checkVirtualList();
                if (index >= this._virtualItems.length)
                    throw new Error("Invalid child index: " + index + ">" + this._virtualItems.length);
                if (this._loop)
                    index = Math.floor(this._firstIndex / this._numItems) * this._numItems + index;
                var rect;
                var ii = this._virtualItems[index];
                var pos = 0;
                var i;
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal) {
                    for (i = this._curLineItemCount - 1; i < index; i += this._curLineItemCount)
                        pos += this._virtualItems[i].height + this._lineGap;
                    rect = new Laya.Rectangle(0, pos, this._itemSize.x, ii.height);
                }
                else if (this._layout == fgui.ListLayoutType.SingleRow || this._layout == fgui.ListLayoutType.FlowVertical) {
                    for (i = this._curLineItemCount - 1; i < index; i += this._curLineItemCount)
                        pos += this._virtualItems[i].width + this._columnGap;
                    rect = new Laya.Rectangle(pos, 0, ii.width, this._itemSize.y);
                }
                else {
                    var page = index / (this._curLineItemCount * this._curLineItemCount2);
                    rect = new Laya.Rectangle(page * this.viewWidth + (index % this._curLineItemCount) * (ii.width + this._columnGap), (index / this._curLineItemCount) % this._curLineItemCount2 * (ii.height + this._lineGap), ii.width, ii.height);
                }
                if (this._scrollPane)
                    this._scrollPane.scrollToView(rect, ani, setFirst);
            }
            else {
                var obj = this.getChildAt(index);
                if (this._scrollPane)
                    this._scrollPane.scrollToView(obj, ani, setFirst);
                else if (this._parent && this._parent.scrollPane)
                    this._parent.scrollPane.scrollToView(obj, ani, setFirst);
            }
        }
        getFirstChildInView() {
            return this.childIndexToItemIndex(super.getFirstChildInView());
        }
        childIndexToItemIndex(index) {
            if (!this._virtual)
                return index;
            if (this._layout == fgui.ListLayoutType.Pagination) {
                for (var i = this._firstIndex; i < this._realNumItems; i++) {
                    if (this._virtualItems[i].obj) {
                        index--;
                        if (index < 0)
                            return i;
                    }
                }
                return index;
            }
            else {
                index += this._firstIndex;
                if (this._loop && this._numItems > 0)
                    index = index % this._numItems;
                return index;
            }
        }
        itemIndexToChildIndex(index) {
            if (!this._virtual)
                return index;
            if (this._layout == fgui.ListLayoutType.Pagination) {
                return this.getChildIndex(this._virtualItems[index].obj);
            }
            else {
                if (this._loop && this._numItems > 0) {
                    var j = this._firstIndex % this._numItems;
                    if (index >= j)
                        index = index - j;
                    else
                        index = this._numItems - j + index;
                }
                else
                    index -= this._firstIndex;
                return index;
            }
        }
        setVirtual() {
            this._setVirtual(false);
        }
        /**
         * Set the list to be virtual list, and has loop behavior.
         */
        setVirtualAndLoop() {
            this._setVirtual(true);
        }
        _setVirtual(loop) {
            if (!this._virtual) {
                if (this._scrollPane == null)
                    throw new Error("Virtual list must be scrollable!");
                if (loop) {
                    if (this._layout == fgui.ListLayoutType.FlowHorizontal || this._layout == fgui.ListLayoutType.FlowVertical)
                        throw new Error("Loop list instanceof not supported for FlowHorizontal or FlowVertical this.layout!");
                    this._scrollPane.bouncebackEffect = false;
                }
                this._virtual = true;
                this._loop = loop;
                this._virtualItems = new Array();
                this.removeChildrenToPool();
                if (this._itemSize == null) {
                    this._itemSize = new Laya.Point();
                    var obj = this.getFromPool(null);
                    if (obj == null) {
                        throw new Error("Virtual List must have a default list item resource.");
                    }
                    else {
                        this._itemSize.x = obj.width;
                        this._itemSize.y = obj.height;
                    }
                    this.returnToPool(obj);
                }
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal) {
                    this._scrollPane.scrollStep = this._itemSize.y;
                    if (this._loop)
                        this._scrollPane._loop = 2;
                }
                else {
                    this._scrollPane.scrollStep = this._itemSize.x;
                    if (this._loop)
                        this._scrollPane._loop = 1;
                }
                this.on(fgui.Events.SCROLL, this, this.__scrolled);
                this.setVirtualListChangedFlag(true);
            }
        }
        /**
         * Set the list item count.
         * If the list instanceof not virtual, specified number of items will be created.
         * If the list instanceof virtual, only items in view will be created.
         */
        get numItems() {
            if (this._virtual)
                return this._numItems;
            else
                return this._children.length;
        }
        set numItems(value) {
            var i;
            if (this._virtual) {
                if (this.itemRenderer == null)
                    throw new Error("set itemRenderer first!");
                this._numItems = value;
                if (this._loop)
                    this._realNumItems = this._numItems * 6; //设置6倍数量，用于循环滚动
                else
                    this._realNumItems = this._numItems;
                //_virtualItems的设计是只增不减的
                var oldCount = this._virtualItems.length;
                if (this._realNumItems > oldCount) {
                    for (i = oldCount; i < this._realNumItems; i++) {
                        var ii = {
                            width: this._itemSize.x,
                            height: this._itemSize.y,
                            updateFlag: 0
                        };
                        this._virtualItems.push(ii);
                    }
                }
                else {
                    for (i = this._realNumItems; i < oldCount; i++)
                        this._virtualItems[i].selected = false;
                }
                if (this._virtualListChanged != 0)
                    Laya.timer.clear(this, this._refreshVirtualList);
                //立即刷新
                this._refreshVirtualList();
            }
            else {
                var cnt = this._children.length;
                if (value > cnt) {
                    for (i = cnt; i < value; i++) {
                        if (this.itemProvider == null) {
                            this.addItemFromPool();
                        }
                        else {
                            if (typeof this.itemProvider === 'function')
                                this.addItemFromPool(this.itemProvider(i));
                            else
                                this.addItemFromPool(this.itemProvider.runWith(i));
                        }
                    }
                }
                else {
                    this.removeChildrenToPool(value, cnt);
                }
                if (this.itemRenderer != null) {
                    for (i = 0; i < value; i++) {
                        if (typeof this.itemRenderer === 'function')
                            this.itemRenderer(i, this.getChildAt(i));
                        else
                            this.itemRenderer.runWith([i, this.getChildAt(i)]);
                    }
                }
            }
        }
        refreshVirtualList() {
            this.setVirtualListChangedFlag(false);
        }
        checkVirtualList() {
            if (this._virtualListChanged != 0) {
                this._refreshVirtualList();
                Laya.timer.clear(this, this._refreshVirtualList);
            }
        }
        setVirtualListChangedFlag(layoutChanged) {
            if (layoutChanged)
                this._virtualListChanged = 2;
            else if (this._virtualListChanged == 0)
                this._virtualListChanged = 1;
            Laya.timer.callLater(this, this._refreshVirtualList);
        }
        _refreshVirtualList() {
            if (!this._displayObject)
                return;
            var layoutChanged = this._virtualListChanged == 2;
            this._virtualListChanged = 0;
            this._eventLocked = true;
            if (layoutChanged) {
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.SingleRow)
                    this._curLineItemCount = 1;
                else if (this._layout == fgui.ListLayoutType.FlowHorizontal) {
                    if (this._columnCount > 0)
                        this._curLineItemCount = this._columnCount;
                    else {
                        this._curLineItemCount = Math.floor((this._scrollPane.viewWidth + this._columnGap) / (this._itemSize.x + this._columnGap));
                        if (this._curLineItemCount <= 0)
                            this._curLineItemCount = 1;
                    }
                }
                else if (this._layout == fgui.ListLayoutType.FlowVertical) {
                    if (this._lineCount > 0)
                        this._curLineItemCount = this._lineCount;
                    else {
                        this._curLineItemCount = Math.floor((this._scrollPane.viewHeight + this._lineGap) / (this._itemSize.y + this._lineGap));
                        if (this._curLineItemCount <= 0)
                            this._curLineItemCount = 1;
                    }
                }
                else //pagination
                 {
                    if (this._columnCount > 0)
                        this._curLineItemCount = this._columnCount;
                    else {
                        this._curLineItemCount = Math.floor((this._scrollPane.viewWidth + this._columnGap) / (this._itemSize.x + this._columnGap));
                        if (this._curLineItemCount <= 0)
                            this._curLineItemCount = 1;
                    }
                    if (this._lineCount > 0)
                        this._curLineItemCount2 = this._lineCount;
                    else {
                        this._curLineItemCount2 = Math.floor((this._scrollPane.viewHeight + this._lineGap) / (this._itemSize.y + this._lineGap));
                        if (this._curLineItemCount2 <= 0)
                            this._curLineItemCount2 = 1;
                    }
                }
            }
            var ch = 0, cw = 0;
            if (this._realNumItems > 0) {
                var i;
                var len = Math.ceil(this._realNumItems / this._curLineItemCount) * this._curLineItemCount;
                var len2 = Math.min(this._curLineItemCount, this._realNumItems);
                if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal) {
                    for (i = 0; i < len; i += this._curLineItemCount)
                        ch += this._virtualItems[i].height + this._lineGap;
                    if (ch > 0)
                        ch -= this._lineGap;
                    if (this._autoResizeItem)
                        cw = this._scrollPane.viewWidth;
                    else {
                        for (i = 0; i < len2; i++)
                            cw += this._virtualItems[i].width + this._columnGap;
                        if (cw > 0)
                            cw -= this._columnGap;
                    }
                }
                else if (this._layout == fgui.ListLayoutType.SingleRow || this._layout == fgui.ListLayoutType.FlowVertical) {
                    for (i = 0; i < len; i += this._curLineItemCount)
                        cw += this._virtualItems[i].width + this._columnGap;
                    if (cw > 0)
                        cw -= this._columnGap;
                    if (this._autoResizeItem)
                        ch = this._scrollPane.viewHeight;
                    else {
                        for (i = 0; i < len2; i++)
                            ch += this._virtualItems[i].height + this._lineGap;
                        if (ch > 0)
                            ch -= this._lineGap;
                    }
                }
                else {
                    var pageCount = Math.ceil(len / (this._curLineItemCount * this._curLineItemCount2));
                    cw = pageCount * this.viewWidth;
                    ch = this.viewHeight;
                }
            }
            this.handleAlign(cw, ch);
            this._scrollPane.setContentSize(cw, ch);
            this._eventLocked = false;
            this.handleScroll(true);
        }
        __scrolled(evt) {
            this.handleScroll(false);
        }
        getIndexOnPos1(forceUpdate) {
            if (this._realNumItems < this._curLineItemCount) {
                s_n = 0;
                return 0;
            }
            var i;
            var pos2;
            var pos3;
            if (this.numChildren > 0 && !forceUpdate) {
                pos2 = this.getChildAt(0).y;
                if (pos2 > s_n) {
                    for (i = this._firstIndex - this._curLineItemCount; i >= 0; i -= this._curLineItemCount) {
                        pos2 -= (this._virtualItems[i].height + this._lineGap);
                        if (pos2 <= s_n) {
                            s_n = pos2;
                            return i;
                        }
                    }
                    s_n = 0;
                    return 0;
                }
                else {
                    for (i = this._firstIndex; i < this._realNumItems; i += this._curLineItemCount) {
                        pos3 = pos2 + this._virtualItems[i].height + this._lineGap;
                        if (pos3 > s_n) {
                            s_n = pos2;
                            return i;
                        }
                        pos2 = pos3;
                    }
                    s_n = pos2;
                    return this._realNumItems - this._curLineItemCount;
                }
            }
            else {
                pos2 = 0;
                for (i = 0; i < this._realNumItems; i += this._curLineItemCount) {
                    pos3 = pos2 + this._virtualItems[i].height + this._lineGap;
                    if (pos3 > s_n) {
                        s_n = pos2;
                        return i;
                    }
                    pos2 = pos3;
                }
                s_n = pos2;
                return this._realNumItems - this._curLineItemCount;
            }
        }
        getIndexOnPos2(forceUpdate) {
            if (this._realNumItems < this._curLineItemCount) {
                s_n = 0;
                return 0;
            }
            var i;
            var pos2;
            var pos3;
            if (this.numChildren > 0 && !forceUpdate) {
                pos2 = this.getChildAt(0).x;
                if (pos2 > s_n) {
                    for (i = this._firstIndex - this._curLineItemCount; i >= 0; i -= this._curLineItemCount) {
                        pos2 -= (this._virtualItems[i].width + this._columnGap);
                        if (pos2 <= s_n) {
                            s_n = pos2;
                            return i;
                        }
                    }
                    s_n = 0;
                    return 0;
                }
                else {
                    for (i = this._firstIndex; i < this._realNumItems; i += this._curLineItemCount) {
                        pos3 = pos2 + this._virtualItems[i].width + this._columnGap;
                        if (pos3 > s_n) {
                            s_n = pos2;
                            return i;
                        }
                        pos2 = pos3;
                    }
                    s_n = pos2;
                    return this._realNumItems - this._curLineItemCount;
                }
            }
            else {
                pos2 = 0;
                for (i = 0; i < this._realNumItems; i += this._curLineItemCount) {
                    pos3 = pos2 + this._virtualItems[i].width + this._columnGap;
                    if (pos3 > s_n) {
                        s_n = pos2;
                        return i;
                    }
                    pos2 = pos3;
                }
                s_n = pos2;
                return this._realNumItems - this._curLineItemCount;
            }
        }
        getIndexOnPos3(forceUpdate) {
            if (this._realNumItems < this._curLineItemCount) {
                s_n = 0;
                return 0;
            }
            var viewWidth = this.viewWidth;
            var page = Math.floor(s_n / viewWidth);
            var startIndex = page * (this._curLineItemCount * this._curLineItemCount2);
            var pos2 = page * viewWidth;
            var i;
            var pos3;
            for (i = 0; i < this._curLineItemCount; i++) {
                pos3 = pos2 + this._virtualItems[startIndex + i].width + this._columnGap;
                if (pos3 > s_n) {
                    s_n = pos2;
                    return startIndex + i;
                }
                pos2 = pos3;
            }
            s_n = pos2;
            return startIndex + this._curLineItemCount - 1;
        }
        handleScroll(forceUpdate) {
            if (this._eventLocked)
                return;
            if (this._layout == fgui.ListLayoutType.SingleColumn || this._layout == fgui.ListLayoutType.FlowHorizontal) {
                var enterCounter = 0;
                while (this.handleScroll1(forceUpdate)) {
                    enterCounter++;
                    forceUpdate = false;
                    if (enterCounter > 20) {
                        console.log("FairyGUI: list will never be <the> filled item renderer function always returns a different size.");
                        break;
                    }
                }
                this.handleArchOrder1();
            }
            else if (this._layout == fgui.ListLayoutType.SingleRow || this._layout == fgui.ListLayoutType.FlowVertical) {
                enterCounter = 0;
                while (this.handleScroll2(forceUpdate)) {
                    enterCounter++;
                    forceUpdate = false;
                    if (enterCounter > 20) {
                        console.log("FairyGUI: list will never be <the> filled item renderer function always returns a different size.");
                        break;
                    }
                }
                this.handleArchOrder2();
            }
            else {
                this.handleScroll3(forceUpdate);
            }
            this._boundsChanged = false;
        }
        handleScroll1(forceUpdate) {
            var pos = this._scrollPane.scrollingPosY;
            var max = pos + this._scrollPane.viewHeight;
            var end = max == this._scrollPane.contentHeight; //这个标志表示当前需要滚动到最末，无论内容变化大小
            //寻找当前位置的第一条项目
            s_n = pos;
            var newFirstIndex = this.getIndexOnPos1(forceUpdate);
            pos = s_n;
            if (newFirstIndex == this._firstIndex && !forceUpdate)
                return false;
            var oldFirstIndex = this._firstIndex;
            this._firstIndex = newFirstIndex;
            var curIndex = newFirstIndex;
            var forward = oldFirstIndex > newFirstIndex;
            var childCount = this.numChildren;
            var lastIndex = oldFirstIndex + childCount - 1;
            var reuseIndex = forward ? lastIndex : oldFirstIndex;
            var curX = 0, curY = pos;
            var needRender;
            var deltaSize = 0;
            var firstItemDeltaSize = 0;
            var url = this._defaultItem;
            var ii, ii2;
            var i, j;
            var partSize = (this._scrollPane.viewWidth - this._columnGap * (this._curLineItemCount - 1)) / this._curLineItemCount;
            this.itemInfoVer++;
            while (curIndex < this._realNumItems && (end || curY < max)) {
                ii = this._virtualItems[curIndex];
                if (ii.obj == null || forceUpdate) {
                    if (this.itemProvider != null) {
                        url = typeof this.itemProvider === 'function' ? this.itemProvider(curIndex % this._numItems) : this.itemProvider.runWith(curIndex % this._numItems);
                        if (url == null)
                            url = this._defaultItem;
                        url = fgui.UIPackage.normalizeURL(url);
                    }
                    if (ii.obj && ii.obj.resourceURL != url) {
                        if (ii.obj instanceof fgui.GButton)
                            ii.selected = ii.obj.selected;
                        this.removeChildToPool(ii.obj);
                        ii.obj = null;
                    }
                }
                if (ii.obj == null) {
                    //搜索最适合的重用item，保证每次刷新需要新建或者重新render的item最少
                    if (forward) {
                        for (j = reuseIndex; j >= oldFirstIndex; j--) {
                            ii2 = this._virtualItems[j];
                            if (ii2.obj && ii2.updateFlag != this.itemInfoVer && ii2.obj.resourceURL == url) {
                                if (ii2.obj instanceof fgui.GButton)
                                    ii2.selected = ii2.obj.selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex--;
                                break;
                            }
                        }
                    }
                    else {
                        for (j = reuseIndex; j <= lastIndex; j++) {
                            ii2 = this._virtualItems[j];
                            if (ii2.obj && ii2.updateFlag != this.itemInfoVer && ii2.obj.resourceURL == url) {
                                if (ii2.obj instanceof fgui.GButton)
                                    ii2.selected = ii2.obj.selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex++;
                                break;
                            }
                        }
                    }
                    if (ii.obj) {
                        this.setChildIndex(ii.obj, forward ? curIndex - newFirstIndex : this.numChildren);
                    }
                    else {
                        ii.obj = this._pool.getObject(url);
                        if (forward)
                            this.addChildAt(ii.obj, curIndex - newFirstIndex);
                        else
                            this.addChild(ii.obj);
                    }
                    if (ii.obj instanceof fgui.GButton)
                        ii.obj.selected = ii.selected;
                    needRender = true;
                }
                else
                    needRender = forceUpdate;
                if (needRender) {
                    if (this._autoResizeItem && (this._layout == fgui.ListLayoutType.SingleColumn || this._columnCount > 0))
                        ii.obj.setSize(partSize, ii.obj.height, true);
                    typeof this.itemRenderer === 'function' ? this.itemRenderer(curIndex % this._numItems, ii.obj) : this.itemRenderer.runWith([curIndex % this._numItems, ii.obj]);
                    if (curIndex % this._curLineItemCount == 0) {
                        deltaSize += Math.ceil(ii.obj.height) - ii.height;
                        if (curIndex == newFirstIndex && oldFirstIndex > newFirstIndex) {
                            //当内容向下滚动时，如果新出现的项目大小发生变化，需要做一个位置补偿，才不会导致滚动跳动
                            firstItemDeltaSize = Math.ceil(ii.obj.height) - ii.height;
                        }
                    }
                    ii.width = Math.ceil(ii.obj.width);
                    ii.height = Math.ceil(ii.obj.height);
                }
                ii.updateFlag = this.itemInfoVer;
                ii.obj.setXY(curX, curY);
                if (curIndex == newFirstIndex) //要显示多一条才不会穿帮
                    max += ii.height;
                curX += ii.width + this._columnGap;
                if (curIndex % this._curLineItemCount == this._curLineItemCount - 1) {
                    curX = 0;
                    curY += ii.height + this._lineGap;
                }
                curIndex++;
            }
            for (i = 0; i < childCount; i++) {
                ii = this._virtualItems[oldFirstIndex + i];
                if (ii.updateFlag != this.itemInfoVer && ii.obj) {
                    if (ii.obj instanceof fgui.GButton)
                        ii.selected = ii.obj.selected;
                    this.removeChildToPool(ii.obj);
                    ii.obj = null;
                }
            }
            childCount = this._children.length;
            for (i = 0; i < childCount; i++) {
                var obj = this._virtualItems[newFirstIndex + i].obj;
                if (this._children[i] != obj)
                    this.setChildIndex(obj, i);
            }
            if (deltaSize != 0 || firstItemDeltaSize != 0)
                this._scrollPane.changeContentSizeOnScrolling(0, deltaSize, 0, firstItemDeltaSize);
            if (curIndex > 0 && this.numChildren > 0 && this._container.y <= 0 && this.getChildAt(0).y > -this._container.y) //最后一页没填满！
                return true;
            else
                return false;
        }
        handleScroll2(forceUpdate) {
            var pos = this._scrollPane.scrollingPosX;
            var max = pos + this._scrollPane.viewWidth;
            var end = pos == this._scrollPane.contentWidth; //这个标志表示当前需要滚动到最末，无论内容变化大小
            //寻找当前位置的第一条项目
            s_n = pos;
            var newFirstIndex = this.getIndexOnPos2(forceUpdate);
            pos = s_n;
            if (newFirstIndex == this._firstIndex && !forceUpdate)
                return false;
            var oldFirstIndex = this._firstIndex;
            this._firstIndex = newFirstIndex;
            var curIndex = newFirstIndex;
            var forward = oldFirstIndex > newFirstIndex;
            var childCount = this.numChildren;
            var lastIndex = oldFirstIndex + childCount - 1;
            var reuseIndex = forward ? lastIndex : oldFirstIndex;
            var curX = pos, curY = 0;
            var needRender;
            var deltaSize = 0;
            var firstItemDeltaSize = 0;
            var url = this._defaultItem;
            var ii, ii2;
            var i, j;
            var partSize = (this._scrollPane.viewHeight - this._lineGap * (this._curLineItemCount - 1)) / this._curLineItemCount;
            this.itemInfoVer++;
            while (curIndex < this._realNumItems && (end || curX < max)) {
                ii = this._virtualItems[curIndex];
                if (ii.obj == null || forceUpdate) {
                    if (this.itemProvider != null) {
                        url = typeof this.itemProvider === 'function' ? this.itemProvider(curIndex % this._numItems) : this.itemProvider.runWith(curIndex % this._numItems);
                        if (url == null)
                            url = this._defaultItem;
                        url = fgui.UIPackage.normalizeURL(url);
                    }
                    if (ii.obj && ii.obj.resourceURL != url) {
                        if (ii.obj instanceof fgui.GButton)
                            ii.selected = ii.obj.selected;
                        this.removeChildToPool(ii.obj);
                        ii.obj = null;
                    }
                }
                if (ii.obj == null) {
                    if (forward) {
                        for (j = reuseIndex; j >= oldFirstIndex; j--) {
                            ii2 = this._virtualItems[j];
                            if (ii2.obj && ii2.updateFlag != this.itemInfoVer && ii2.obj.resourceURL == url) {
                                if (ii2.obj instanceof fgui.GButton)
                                    ii2.selected = ii2.obj.selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex--;
                                break;
                            }
                        }
                    }
                    else {
                        for (j = reuseIndex; j <= lastIndex; j++) {
                            ii2 = this._virtualItems[j];
                            if (ii2.obj && ii2.updateFlag != this.itemInfoVer && ii2.obj.resourceURL == url) {
                                if (ii2.obj instanceof fgui.GButton)
                                    ii2.selected = ii2.obj.selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex++;
                                break;
                            }
                        }
                    }
                    if (ii.obj) {
                        this.setChildIndex(ii.obj, forward ? curIndex - newFirstIndex : this.numChildren);
                    }
                    else {
                        ii.obj = this._pool.getObject(url);
                        if (forward)
                            this.addChildAt(ii.obj, curIndex - newFirstIndex);
                        else
                            this.addChild(ii.obj);
                    }
                    if (ii.obj instanceof fgui.GButton)
                        ii.obj.selected = ii.selected;
                    needRender = true;
                }
                else
                    needRender = forceUpdate;
                if (needRender) {
                    if (this._autoResizeItem && (this._layout == fgui.ListLayoutType.SingleRow || this._lineCount > 0))
                        ii.obj.setSize(ii.obj.width, partSize, true);
                    typeof this.itemRenderer === 'function' ? this.itemRenderer(curIndex % this._numItems, ii.obj) : this.itemRenderer.runWith([curIndex % this._numItems, ii.obj]);
                    if (curIndex % this._curLineItemCount == 0) {
                        deltaSize += Math.ceil(ii.obj.width) - ii.width;
                        if (curIndex == newFirstIndex && oldFirstIndex > newFirstIndex) {
                            //当内容向下滚动时，如果新出现的一个项目大小发生变化，需要做一个位置补偿，才不会导致滚动跳动
                            firstItemDeltaSize = Math.ceil(ii.obj.width) - ii.width;
                        }
                    }
                    ii.width = Math.ceil(ii.obj.width);
                    ii.height = Math.ceil(ii.obj.height);
                }
                ii.updateFlag = this.itemInfoVer;
                ii.obj.setXY(curX, curY);
                if (curIndex == newFirstIndex) //要显示多一条才不会穿帮
                    max += ii.width;
                curY += ii.height + this._lineGap;
                if (curIndex % this._curLineItemCount == this._curLineItemCount - 1) {
                    curY = 0;
                    curX += ii.width + this._columnGap;
                }
                curIndex++;
            }
            for (i = 0; i < childCount; i++) {
                ii = this._virtualItems[oldFirstIndex + i];
                if (ii.updateFlag != this.itemInfoVer && ii.obj) {
                    if (ii.obj instanceof fgui.GButton)
                        ii.selected = ii.obj.selected;
                    this.removeChildToPool(ii.obj);
                    ii.obj = null;
                }
            }
            childCount = this._children.length;
            for (i = 0; i < childCount; i++) {
                var obj = this._virtualItems[newFirstIndex + i].obj;
                if (this._children[i] != obj)
                    this.setChildIndex(obj, i);
            }
            if (deltaSize != 0 || firstItemDeltaSize != 0)
                this._scrollPane.changeContentSizeOnScrolling(deltaSize, 0, firstItemDeltaSize, 0);
            if (curIndex > 0 && this.numChildren > 0 && this._container.x <= 0 && this.getChildAt(0).x > -this._container.x) //最后一页没填满！
                return true;
            else
                return false;
        }
        handleScroll3(forceUpdate) {
            var pos = this._scrollPane.scrollingPosX;
            //寻找当前位置的第一条项目
            s_n = pos;
            var newFirstIndex = this.getIndexOnPos3(forceUpdate);
            pos = s_n;
            if (newFirstIndex == this._firstIndex && !forceUpdate)
                return;
            var oldFirstIndex = this._firstIndex;
            this._firstIndex = newFirstIndex;
            //分页模式不支持不等高，所以渲染满一页就好了
            var reuseIndex = oldFirstIndex;
            var virtualItemCount = this._virtualItems.length;
            var pageSize = this._curLineItemCount * this._curLineItemCount2;
            var startCol = newFirstIndex % this._curLineItemCount;
            var viewWidth = this.viewWidth;
            var page = Math.floor(newFirstIndex / pageSize);
            var startIndex = page * pageSize;
            var lastIndex = startIndex + pageSize * 2; //测试两页
            var needRender;
            var i;
            var ii, ii2;
            var col;
            var url = this._defaultItem;
            var partWidth = (this._scrollPane.viewWidth - this._columnGap * (this._curLineItemCount - 1)) / this._curLineItemCount;
            var partHeight = (this._scrollPane.viewHeight - this._lineGap * (this._curLineItemCount2 - 1)) / this._curLineItemCount2;
            this.itemInfoVer++;
            //先标记这次要用到的项目
            for (i = startIndex; i < lastIndex; i++) {
                if (i >= this._realNumItems)
                    continue;
                col = i % this._curLineItemCount;
                if (i - startIndex < pageSize) {
                    if (col < startCol)
                        continue;
                }
                else {
                    if (col > startCol)
                        continue;
                }
                ii = this._virtualItems[i];
                ii.updateFlag = this.itemInfoVer;
            }
            var lastObj = null;
            var insertIndex = 0;
            for (i = startIndex; i < lastIndex; i++) {
                if (i >= this._realNumItems)
                    continue;
                ii = this._virtualItems[i];
                if (ii.updateFlag != this.itemInfoVer)
                    continue;
                if (ii.obj == null) {
                    //寻找看有没有可重用的
                    while (reuseIndex < virtualItemCount) {
                        ii2 = this._virtualItems[reuseIndex];
                        if (ii2.obj && ii2.updateFlag != this.itemInfoVer) {
                            if (ii2.obj instanceof fgui.GButton)
                                ii2.selected = ii2.obj.selected;
                            ii.obj = ii2.obj;
                            ii2.obj = null;
                            break;
                        }
                        reuseIndex++;
                    }
                    if (insertIndex == -1)
                        insertIndex = this.getChildIndex(lastObj) + 1;
                    if (ii.obj == null) {
                        if (this.itemProvider != null) {
                            url = typeof this.itemProvider === 'function' ? this.itemProvider(i % this._numItems) : this.itemProvider.runWith(i % this._numItems);
                            if (url == null)
                                url = this._defaultItem;
                            url = fgui.UIPackage.normalizeURL(url);
                        }
                        ii.obj = this._pool.getObject(url);
                        this.addChildAt(ii.obj, insertIndex);
                    }
                    else {
                        insertIndex = this.setChildIndexBefore(ii.obj, insertIndex);
                    }
                    insertIndex++;
                    if (ii.obj instanceof fgui.GButton)
                        ii.obj.selected = ii.selected;
                    needRender = true;
                }
                else {
                    needRender = forceUpdate;
                    insertIndex = -1;
                    lastObj = ii.obj;
                }
                if (needRender) {
                    if (this._autoResizeItem) {
                        if (this._curLineItemCount == this._columnCount && this._curLineItemCount2 == this._lineCount)
                            ii.obj.setSize(partWidth, partHeight, true);
                        else if (this._curLineItemCount == this._columnCount)
                            ii.obj.setSize(partWidth, ii.obj.height, true);
                        else if (this._curLineItemCount2 == this._lineCount)
                            ii.obj.setSize(ii.obj.width, partHeight, true);
                    }
                    typeof this.itemRenderer === 'function' ? this.itemRenderer(i % this._numItems, ii.obj) : this.itemRenderer.runWith([i % this._numItems, ii.obj]);
                    ii.width = Math.ceil(ii.obj.width);
                    ii.height = Math.ceil(ii.obj.height);
                }
            }
            //排列item
            var borderX = (startIndex / pageSize) * viewWidth;
            var xx = borderX;
            var yy = 0;
            var lineHeight = 0;
            for (i = startIndex; i < lastIndex; i++) {
                if (i >= this._realNumItems)
                    continue;
                ii = this._virtualItems[i];
                if (ii.updateFlag == this.itemInfoVer)
                    ii.obj.setXY(xx, yy);
                if (ii.height > lineHeight)
                    lineHeight = ii.height;
                if (i % this._curLineItemCount == this._curLineItemCount - 1) {
                    xx = borderX;
                    yy += lineHeight + this._lineGap;
                    lineHeight = 0;
                    if (i == startIndex + pageSize - 1) {
                        borderX += viewWidth;
                        xx = borderX;
                        yy = 0;
                    }
                }
                else
                    xx += ii.width + this._columnGap;
            }
            //释放未使用的
            for (i = reuseIndex; i < virtualItemCount; i++) {
                ii = this._virtualItems[i];
                if (ii.updateFlag != this.itemInfoVer && ii.obj) {
                    if (ii.obj instanceof fgui.GButton)
                        ii.selected = ii.obj.selected;
                    this.removeChildToPool(ii.obj);
                    ii.obj = null;
                }
            }
        }
        handleArchOrder1() {
            if (this.childrenRenderOrder == fgui.ChildrenRenderOrder.Arch) {
                var mid = this._scrollPane.posY + this.viewHeight / 2;
                var minDist = Number.POSITIVE_INFINITY;
                var dist = 0;
                var apexIndex = 0;
                var cnt = this.numChildren;
                for (var i = 0; i < cnt; i++) {
                    var obj = this.getChildAt(i);
                    if (!this.foldInvisibleItems || obj.visible) {
                        dist = Math.abs(mid - obj.y - obj.height / 2);
                        if (dist < minDist) {
                            minDist = dist;
                            apexIndex = i;
                        }
                    }
                }
                this.apexIndex = apexIndex;
            }
        }
        handleArchOrder2() {
            if (this.childrenRenderOrder == fgui.ChildrenRenderOrder.Arch) {
                var mid = this._scrollPane.posX + this.viewWidth / 2;
                var minDist = Number.POSITIVE_INFINITY;
                var dist = 0;
                var apexIndex = 0;
                var cnt = this.numChildren;
                for (var i = 0; i < cnt; i++) {
                    var obj = this.getChildAt(i);
                    if (!this.foldInvisibleItems || obj.visible) {
                        dist = Math.abs(mid - obj.x - obj.width / 2);
                        if (dist < minDist) {
                            minDist = dist;
                            apexIndex = i;
                        }
                    }
                }
                this.apexIndex = apexIndex;
            }
        }
        handleAlign(contentWidth, contentHeight) {
            var newOffsetX = 0;
            var newOffsetY = 0;
            if (contentHeight < this.viewHeight) {
                if (this._verticalAlign == "middle")
                    newOffsetY = Math.floor((this.viewHeight - contentHeight) / 2);
                else if (this._verticalAlign == "bottom")
                    newOffsetY = this.viewHeight - contentHeight;
            }
            if (contentWidth < this.viewWidth) {
                if (this._align == "center")
                    newOffsetX = Math.floor((this.viewWidth - contentWidth) / 2);
                else if (this._align == "right")
                    newOffsetX = this.viewWidth - contentWidth;
            }
            if (newOffsetX != this._alignOffset.x || newOffsetY != this._alignOffset.y) {
                this._alignOffset.setTo(newOffsetX, newOffsetY);
                if (this._scrollPane)
                    this._scrollPane.adjustMaskContainer();
                else
                    this._container.pos(this._margin.left + this._alignOffset.x, this._margin.top + this._alignOffset.y);
            }
        }
        updateBounds() {
            if (this._virtual)
                return;
            var i;
            var child;
            var curX = 0;
            var curY = 0;
            var maxWidth = 0;
            var maxHeight = 0;
            var cw, ch;
            var j = 0;
            var page = 0;
            var k = 0;
            var cnt = this._children.length;
            var viewWidth = this.viewWidth;
            var viewHeight = this.viewHeight;
            var lineSize = 0;
            var lineStart = 0;
            var ratio;
            if (this._layout == fgui.ListLayoutType.SingleColumn) {
                for (i = 0; i < cnt; i++) {
                    child = this.getChildAt(i);
                    if (this.foldInvisibleItems && !child.visible)
                        continue;
                    if (curY != 0)
                        curY += this._lineGap;
                    child.y = curY;
                    if (this._autoResizeItem)
                        child.setSize(viewWidth, child.height, true);
                    curY += Math.ceil(child.height);
                    if (child.width > maxWidth)
                        maxWidth = child.width;
                }
                ch = curY;
                if (ch <= viewHeight && this._autoResizeItem && this._scrollPane && this._scrollPane._displayInDemand && this._scrollPane.vtScrollBar) {
                    viewWidth += this._scrollPane.vtScrollBar.width;
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        child.setSize(viewWidth, child.height, true);
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                    }
                }
                cw = Math.ceil(maxWidth);
            }
            else if (this._layout == fgui.ListLayoutType.SingleRow) {
                for (i = 0; i < cnt; i++) {
                    child = this.getChildAt(i);
                    if (this.foldInvisibleItems && !child.visible)
                        continue;
                    if (curX != 0)
                        curX += this._columnGap;
                    child.x = curX;
                    if (this._autoResizeItem)
                        child.setSize(child.width, viewHeight, true);
                    curX += Math.ceil(child.width);
                    if (child.height > maxHeight)
                        maxHeight = child.height;
                }
                cw = curX;
                if (cw <= viewWidth && this._autoResizeItem && this._scrollPane && this._scrollPane._displayInDemand && this._scrollPane.hzScrollBar) {
                    viewHeight += this._scrollPane.hzScrollBar.height;
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        child.setSize(child.width, viewHeight, true);
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                    }
                }
                ch = Math.ceil(maxHeight);
            }
            else if (this._layout == fgui.ListLayoutType.FlowHorizontal) {
                if (this._autoResizeItem && this._columnCount > 0) {
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        lineSize += child.sourceWidth;
                        j++;
                        if (j == this._columnCount || i == cnt - 1) {
                            ratio = (viewWidth - lineSize - (j - 1) * this._columnGap) / lineSize;
                            curX = 0;
                            for (j = lineStart; j <= i; j++) {
                                child = this.getChildAt(j);
                                if (this.foldInvisibleItems && !child.visible)
                                    continue;
                                child.setXY(curX, curY);
                                if (j < i) {
                                    child.setSize(child.sourceWidth + Math.round(child.sourceWidth * ratio), child.height, true);
                                    curX += Math.ceil(child.width) + this._columnGap;
                                }
                                else {
                                    child.setSize(viewWidth - curX, child.height, true);
                                }
                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Math.ceil(maxHeight) + this._lineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    ch = curY + Math.ceil(maxHeight);
                    cw = viewWidth;
                }
                else {
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        if (curX != 0)
                            curX += this._columnGap;
                        if (this._columnCount != 0 && j >= this._columnCount
                            || this._columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0) {
                            //new line
                            curX = 0;
                            curY += Math.ceil(maxHeight) + this._lineGap;
                            maxHeight = 0;
                            j = 0;
                        }
                        child.setXY(curX, curY);
                        curX += Math.ceil(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                    ch = curY + Math.ceil(maxHeight);
                    cw = Math.ceil(maxWidth);
                }
            }
            else if (this._layout == fgui.ListLayoutType.FlowVertical) {
                if (this._autoResizeItem && this._lineCount > 0) {
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        lineSize += child.sourceHeight;
                        j++;
                        if (j == this._lineCount || i == cnt - 1) {
                            ratio = (viewHeight - lineSize - (j - 1) * this._lineGap) / lineSize;
                            curY = 0;
                            for (j = lineStart; j <= i; j++) {
                                child = this.getChildAt(j);
                                if (this.foldInvisibleItems && !child.visible)
                                    continue;
                                child.setXY(curX, curY);
                                if (j < i) {
                                    child.setSize(child.width, child.sourceHeight + Math.round(child.sourceHeight * ratio), true);
                                    curY += Math.ceil(child.height) + this._lineGap;
                                }
                                else {
                                    child.setSize(child.width, viewHeight - curY, true);
                                }
                                if (child.width > maxWidth)
                                    maxWidth = child.width;
                            }
                            //new line
                            curX += Math.ceil(maxWidth) + this._columnGap;
                            maxWidth = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    cw = curX + Math.ceil(maxWidth);
                    ch = viewHeight;
                }
                else {
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        if (curY != 0)
                            curY += this._lineGap;
                        if (this._lineCount != 0 && j >= this._lineCount
                            || this._lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0) {
                            curY = 0;
                            curX += Math.ceil(maxWidth) + this._columnGap;
                            maxWidth = 0;
                            j = 0;
                        }
                        child.setXY(curX, curY);
                        curY += Math.ceil(child.height);
                        if (curY > maxHeight)
                            maxHeight = curY;
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                        j++;
                    }
                    cw = curX + Math.ceil(maxWidth);
                    ch = Math.ceil(maxHeight);
                }
            }
            else //pagination
             {
                var eachHeight;
                if (this._autoResizeItem && this._lineCount > 0)
                    eachHeight = Math.floor((viewHeight - (this._lineCount - 1) * this._lineGap) / this._lineCount);
                if (this._autoResizeItem && this._columnCount > 0) {
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        if (j == 0 && (this._lineCount != 0 && k >= this._lineCount
                            || this._lineCount == 0 && curY + child.height > viewHeight)) {
                            //new page
                            page++;
                            curY = 0;
                            k = 0;
                        }
                        lineSize += child.sourceWidth;
                        j++;
                        if (j == this._columnCount || i == cnt - 1) {
                            ratio = (viewWidth - lineSize - (j - 1) * this._columnGap) / lineSize;
                            curX = 0;
                            for (j = lineStart; j <= i; j++) {
                                child = this.getChildAt(j);
                                if (this.foldInvisibleItems && !child.visible)
                                    continue;
                                child.setXY(page * viewWidth + curX, curY);
                                if (j < i) {
                                    child.setSize(child.sourceWidth + Math.round(child.sourceWidth * ratio), this._lineCount > 0 ? eachHeight : child.height, true);
                                    curX += Math.ceil(child.width) + this._columnGap;
                                }
                                else {
                                    child.setSize(viewWidth - curX, this._lineCount > 0 ? eachHeight : child.height, true);
                                }
                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Math.ceil(maxHeight) + this._lineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                            k++;
                        }
                    }
                }
                else {
                    for (i = 0; i < cnt; i++) {
                        child = this.getChildAt(i);
                        if (this.foldInvisibleItems && !child.visible)
                            continue;
                        if (curX != 0)
                            curX += this._columnGap;
                        if (this._autoResizeItem && this._lineCount > 0)
                            child.setSize(child.width, eachHeight, true);
                        if (this._columnCount != 0 && j >= this._columnCount
                            || this._columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0) {
                            //new line
                            curX = 0;
                            curY += Math.ceil(maxHeight) + this._lineGap;
                            maxHeight = 0;
                            j = 0;
                            k++;
                            if (this._lineCount != 0 && k >= this._lineCount
                                || this._lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0) //new page
                             {
                                page++;
                                curY = 0;
                                k = 0;
                            }
                        }
                        child.setXY(page * viewWidth + curX, curY);
                        curX += Math.ceil(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                }
                ch = page > 0 ? viewHeight : curY + Math.ceil(maxHeight);
                cw = (page + 1) * viewWidth;
            }
            this.handleAlign(cw, ch);
            this.setBounds(0, 0, cw, ch);
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            var i1;
            this._layout = buffer.readByte();
            this._selectionMode = buffer.readByte();
            i1 = buffer.readByte();
            this._align = i1 == 0 ? "left" : (i1 == 1 ? "center" : "right");
            i1 = buffer.readByte();
            this._verticalAlign = i1 == 0 ? "top" : (i1 == 1 ? "middle" : "bottom");
            this._lineGap = buffer.getInt16();
            this._columnGap = buffer.getInt16();
            this._lineCount = buffer.getInt16();
            this._columnCount = buffer.getInt16();
            this._autoResizeItem = buffer.readBool();
            this._childrenRenderOrder = buffer.readByte();
            this._apexIndex = buffer.getInt16();
            if (buffer.readBool()) {
                this._margin.top = buffer.getInt32();
                this._margin.bottom = buffer.getInt32();
                this._margin.left = buffer.getInt32();
                this._margin.right = buffer.getInt32();
            }
            var overflow = buffer.readByte();
            if (overflow == fgui.OverflowType.Scroll) {
                var savedPos = buffer.pos;
                buffer.seek(beginPos, 7);
                this.setupScroll(buffer);
                buffer.pos = savedPos;
            }
            else
                this.setupOverflow(overflow);
            if (buffer.readBool()) //clipSoftness
                buffer.skip(8);
            if (buffer.version >= 2) {
                this.scrollItemToViewOnClick = buffer.readBool();
                this.foldInvisibleItems = buffer.readBool();
            }
            buffer.seek(beginPos, 8);
            this._defaultItem = buffer.readS();
            this.readItems(buffer);
        }
        readItems(buffer) {
            var cnt;
            var i;
            var nextPos;
            var str;
            cnt = buffer.getInt16();
            for (i = 0; i < cnt; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                str = buffer.readS();
                if (str == null) {
                    str = this._defaultItem;
                    if (!str) {
                        buffer.pos = nextPos;
                        continue;
                    }
                }
                var obj = this.getFromPool(str);
                if (obj) {
                    this.addChild(obj);
                    this.setupItem(buffer, obj);
                }
                buffer.pos = nextPos;
            }
        }
        setupItem(buffer, obj) {
            var str;
            str = buffer.readS();
            if (str != null)
                obj.text = str;
            str = buffer.readS();
            if (str != null && (obj instanceof fgui.GButton))
                obj.selectedTitle = str;
            str = buffer.readS();
            if (str != null)
                obj.icon = str;
            str = buffer.readS();
            if (str != null && (obj instanceof fgui.GButton))
                obj.selectedIcon = str;
            str = buffer.readS();
            if (str != null)
                obj.name = str;
            var cnt;
            var i;
            if (obj instanceof fgui.GComponent) {
                cnt = buffer.getInt16();
                for (i = 0; i < cnt; i++) {
                    var cc = obj.getController(buffer.readS());
                    str = buffer.readS();
                    if (cc)
                        cc.selectedPageId = str;
                }
                if (buffer.version >= 2) {
                    cnt = buffer.getInt16();
                    for (i = 0; i < cnt; i++) {
                        var target = buffer.readS();
                        var propertyId = buffer.getInt16();
                        var value = buffer.readS();
                        var obj2 = obj.getChildByPath(target);
                        if (obj2)
                            obj2.setProp(propertyId, value);
                    }
                }
            }
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            buffer.seek(beginPos, 6);
            var i = buffer.getInt16();
            if (i != -1)
                this._selectionController = this._parent.getControllerAt(i);
        }
    }
    fgui.GList = GList;
    var s_n = 0;
})(fgui);

(function (fgui) {
    class GObjectPool {
        constructor() {
            this._count = 0;
            this._pool = {};
        }
        clear() {
            for (var i1 in this._pool) {
                var arr = this._pool[i1];
                var cnt = arr.length;
                for (var i = 0; i < cnt; i++)
                    arr[i].dispose();
            }
            this._pool = {};
            this._count = 0;
        }
        get count() {
            return this._count;
        }
        getObject(url) {
            url = fgui.UIPackage.normalizeURL(url);
            if (url == null)
                return null;
            var arr = this._pool[url];
            if (arr && arr.length > 0) {
                this._count--;
                return arr.shift();
            }
            var child = fgui.UIPackage.createObjectFromURL(url);
            return child;
        }
        returnObject(obj) {
            var url = obj.resourceURL;
            if (!url)
                return;
            var arr = this._pool[url];
            if (arr == null) {
                arr = [];
                this._pool[url] = arr;
            }
            this._count++;
            arr.push(obj);
        }
    }
    fgui.GObjectPool = GObjectPool;
})(fgui);
///<reference path="GObjectPool.ts"/>

///<reference path="GObjectPool.ts"/>
(function (fgui) {
    class GLoader extends fgui.GObject {
        constructor() {
            super();
            this._url = "";
            this._fill = fgui.LoaderFillType.None;
            this._align = "left";
            this._valign = "top";
            this._showErrorSign = true;
        }
        createDisplayObject() {
            super.createDisplayObject();
            this._content = new fgui.MovieClip();
            this._displayObject.addChild(this._content);
            this._displayObject.mouseEnabled = true;
        }
        dispose() {
            if (!this._contentItem && this._content.texture) {
                this.freeExternal(this._content.texture);
            }
            if (this._content2)
                this._content2.dispose();
            super.dispose();
        }
        get url() {
            return this._url;
        }
        set url(value) {
            if (this._url == value)
                return;
            this._url = value;
            this.loadContent();
            this.updateGear(7);
        }
        get icon() {
            return this._url;
        }
        set icon(value) {
            this.url = value;
        }
        get align() {
            return this._align;
        }
        set align(value) {
            if (this._align != value) {
                this._align = value;
                this.updateLayout();
            }
        }
        get verticalAlign() {
            return this._valign;
        }
        set verticalAlign(value) {
            if (this._valign != value) {
                this._valign = value;
                this.updateLayout();
            }
        }
        get fill() {
            return this._fill;
        }
        set fill(value) {
            if (this._fill != value) {
                this._fill = value;
                this.updateLayout();
            }
        }
        get shrinkOnly() {
            return this._shrinkOnly;
        }
        set shrinkOnly(value) {
            if (this._shrinkOnly != value) {
                this._shrinkOnly = value;
                this.updateLayout();
            }
        }
        get autoSize() {
            return this._autoSize;
        }
        set autoSize(value) {
            if (this._autoSize != value) {
                this._autoSize = value;
                this.updateLayout();
            }
        }
        get playing() {
            return this._content.playing;
        }
        set playing(value) {
            if (this._content.playing != value) {
                this._content.playing = value;
                this.updateGear(5);
            }
        }
        get frame() {
            return this._content.frame;
        }
        set frame(value) {
            if (this._content.frame != value) {
                this._content.frame = value;
                this.updateGear(5);
            }
        }
        get color() {
            return this._content.color;
        }
        set color(value) {
            if (this._content.color != value) {
                this._content.color = value;
                this.updateGear(4);
            }
        }
        get fillMethod() {
            return this._content.fillMethod;
        }
        set fillMethod(value) {
            this._content.fillMethod = value;
        }
        get fillOrigin() {
            return this._content.fillOrigin;
        }
        set fillOrigin(value) {
            this._content.fillOrigin = value;
        }
        get fillClockwise() {
            return this._content.fillClockwise;
        }
        set fillClockwise(value) {
            this._content.fillClockwise = value;
        }
        get fillAmount() {
            return this._content.fillAmount;
        }
        set fillAmount(value) {
            this._content.fillAmount = value;
        }
        get showErrorSign() {
            return this._showErrorSign;
        }
        set showErrorSign(value) {
            this._showErrorSign = value;
        }
        get content() {
            return this._content;
        }
        get component() {
            return this._content2;
        }
        loadContent() {
            this.clearContent();
            if (!this._url)
                return;
            if (fgui.ToolSet.startsWith(this._url, "ui://"))
                this.loadFromPackage(this._url);
            else
                this.loadExternal();
        }
        loadFromPackage(itemURL) {
            this._contentItem = fgui.UIPackage.getItemByURL(itemURL);
            if (this._contentItem) {
                this._contentItem = this._contentItem.getBranch();
                this.sourceWidth = this._contentItem.width;
                this.sourceHeight = this._contentItem.height;
                this._contentItem = this._contentItem.getHighResolution();
                this._contentItem.load();
                if (this._autoSize)
                    this.setSize(this.sourceWidth, this.sourceHeight);
                if (this._contentItem.type == fgui.PackageItemType.Image) {
                    if (!this._contentItem.texture) {
                        this.setErrorState();
                    }
                    else {
                        this._content.texture = this._contentItem.texture;
                        this._content.scale9Grid = this._contentItem.scale9Grid;
                        this._content.scaleByTile = this._contentItem.scaleByTile;
                        this._content.tileGridIndice = this._contentItem.tileGridIndice;
                        this.sourceWidth = this._contentItem.width;
                        this.sourceHeight = this._contentItem.height;
                        this.updateLayout();
                    }
                }
                else if (this._contentItem.type == fgui.PackageItemType.MovieClip) {
                    this.sourceWidth = this._contentItem.width;
                    this.sourceHeight = this._contentItem.height;
                    this._content.interval = this._contentItem.interval;
                    this._content.swing = this._contentItem.swing;
                    this._content.repeatDelay = this._contentItem.repeatDelay;
                    this._content.frames = this._contentItem.frames;
                    this.updateLayout();
                }
                else if (this._contentItem.type == fgui.PackageItemType.Component) {
                    var obj = fgui.UIPackage.createObjectFromURL(itemURL);
                    if (!obj)
                        this.setErrorState();
                    else if (!(obj instanceof fgui.GComponent)) {
                        obj.dispose();
                        this.setErrorState();
                    }
                    else {
                        this._content2 = obj.asCom;
                        this._displayObject.addChild(this._content2.displayObject);
                        this.updateLayout();
                    }
                }
                else
                    this.setErrorState();
            }
            else
                this.setErrorState();
        }
        loadExternal() {
            fgui.AssetProxy.inst.load(this._url, Laya.Loader.IMAGE).then((tex) => {
                this.__getResCompleted(tex);
            });
        }
        freeExternal(texture) {
        }
        onExternalLoadSuccess(texture) {
            this._content.texture = texture;
            this._content.scale9Grid = null;
            this._content.scaleByTile = false;
            this.sourceWidth = texture.width;
            this.sourceHeight = texture.height;
            this.updateLayout();
        }
        onExternalLoadFailed() {
            this._url = "";
            this.setErrorState();
        }
        __getResCompleted(tex) {
            if (tex != null)
                this.onExternalLoadSuccess(tex);
            else
                this.onExternalLoadFailed();
        }
        setErrorState() {
            if (!this._showErrorSign)
                return;
            if (!this._errorSign) {
                if (fgui.UIConfig.loaderErrorSign != null) {
                    this._errorSign = GLoader._errorSignPool.getObject(fgui.UIConfig.loaderErrorSign);
                }
            }
            if (this._errorSign) {
                this._errorSign.setSize(this.width, this.height);
                this._displayObject.addChild(this._errorSign.displayObject);
            }
        }
        clearErrorState() {
            if (this._errorSign) {
                this._displayObject.removeChild(this._errorSign.displayObject);
                GLoader._errorSignPool.returnObject(this._errorSign);
                this._errorSign = null;
            }
        }
        updateLayout() {
            if (!this._content2 && !this._content.texture && !this._content.frames) {
                if (this._autoSize) {
                    this._updatingLayout = true;
                    this.setSize(50, 30);
                    this._updatingLayout = false;
                }
                return;
            }
            let cw = this.sourceWidth;
            let ch = this.sourceHeight;
            if (this._autoSize) {
                this._updatingLayout = true;
                if (cw == 0)
                    cw = 50;
                if (ch == 0)
                    ch = 30;
                this.setSize(cw, ch);
                this._updatingLayout = false;
                if (cw == this._width && ch == this._height) {
                    if (this._content2) {
                        this._content2.setXY(0, 0);
                        this._content2.setScale(1, 1);
                    }
                    else {
                        this._content.size(cw, ch);
                        this._content.pos(0, 0);
                    }
                    return;
                }
            }
            var sx = 1, sy = 1;
            if (this._fill != fgui.LoaderFillType.None) {
                sx = this.width / this.sourceWidth;
                sy = this.height / this.sourceHeight;
                if (sx != 1 || sy != 1) {
                    if (this._fill == fgui.LoaderFillType.ScaleMatchHeight)
                        sx = sy;
                    else if (this._fill == fgui.LoaderFillType.ScaleMatchWidth)
                        sy = sx;
                    else if (this._fill == fgui.LoaderFillType.Scale) {
                        if (sx > sy)
                            sx = sy;
                        else
                            sy = sx;
                    }
                    else if (this._fill == fgui.LoaderFillType.ScaleNoBorder) {
                        if (sx > sy)
                            sy = sx;
                        else
                            sx = sy;
                    }
                    if (this._shrinkOnly) {
                        if (sx > 1)
                            sx = 1;
                        if (sy > 1)
                            sy = 1;
                    }
                    cw = this.sourceWidth * sx;
                    ch = this.sourceHeight * sy;
                }
            }
            if (this._content2) {
                if (this._fill == fgui.LoaderFillType.Resize) {
                    this._content2.setSize(cw, ch);
                }
                else {
                    this._content2.setScale(sx, sy);
                }
            }
            else {
                this._content.size(cw, ch);
            }
            var nx, ny;
            if (this._align == "center")
                nx = Math.floor((this.width - cw) / 2);
            else if (this._align == "right")
                nx = this.width - cw;
            else
                nx = 0;
            if (this._valign == "middle")
                ny = Math.floor((this.height - ch) / 2);
            else if (this._valign == "bottom")
                ny = this.height - ch;
            else
                ny = 0;
            if (this._content2)
                this._content2.setXY(nx, ny);
            else
                this._content.pos(nx, ny);
        }
        clearContent() {
            this.clearErrorState();
            if (!this._contentItem && this._content.texture) {
                this.freeExternal(this._content.texture);
            }
            this._content.texture = null;
            this._content.frames = null;
            if (this._content2) {
                this._content2.dispose();
                this._content2 = null;
            }
            this._contentItem = null;
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            if (!this._updatingLayout)
                this.updateLayout();
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.color;
                case fgui.ObjectPropID.Playing:
                    return this.playing;
                case fgui.ObjectPropID.Frame:
                    return this.frame;
                case fgui.ObjectPropID.TimeScale:
                    return this._content.timeScale;
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.color = value;
                    break;
                case fgui.ObjectPropID.Playing:
                    this.playing = value;
                    break;
                case fgui.ObjectPropID.Frame:
                    this.frame = value;
                    break;
                case fgui.ObjectPropID.TimeScale:
                    this._content.timeScale = value;
                    break;
                case fgui.ObjectPropID.DeltaTime:
                    this._content.advance(value);
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            var iv;
            this._url = buffer.readS();
            iv = buffer.readByte();
            this._align = iv == 0 ? "left" : (iv == 1 ? "center" : "right");
            iv = buffer.readByte();
            this._valign = iv == 0 ? "top" : (iv == 1 ? "middle" : "bottom");
            this._fill = buffer.readByte();
            this._shrinkOnly = buffer.readBool();
            this._autoSize = buffer.readBool();
            this._showErrorSign = buffer.readBool();
            this._content.playing = buffer.readBool();
            this._content.frame = buffer.getInt32();
            if (buffer.readBool())
                this.color = buffer.readColorS();
            this._content.fillMethod = buffer.readByte();
            if (this._content.fillMethod != 0) {
                this._content.fillOrigin = buffer.readByte();
                this._content.fillClockwise = buffer.readBool();
                this._content.fillAmount = buffer.getFloat32();
            }
            if (this._url)
                this.loadContent();
        }
    }
    GLoader._errorSignPool = new fgui.GObjectPool();
    fgui.GLoader = GLoader;
})(fgui);

(function (fgui) {
    class GLoader3D extends fgui.GObject {
        constructor() {
            super();
            this._frame = 0;
            this._playing = true;
            this._url = "";
            this._fill = fgui.LoaderFillType.None;
            this._align = fgui.AlignType.Left;
            this._verticalAlign = fgui.VertAlignType.Top;
            this._color = "#FFFFFF";
        }
        createDisplayObject() {
            super.createDisplayObject();
            this._container = new Laya.Sprite();
            this._displayObject.addChild(this._container);
        }
        dispose() {
            super.dispose();
        }
        get url() {
            return this._url;
        }
        set url(value) {
            if (this._url == value)
                return;
            this._url = value;
            this.loadContent();
            this.updateGear(7);
        }
        get icon() {
            return this._url;
        }
        set icon(value) {
            this.url = value;
        }
        get align() {
            return this._align;
        }
        set align(value) {
            if (this._align != value) {
                this._align = value;
                this.updateLayout();
            }
        }
        get verticalAlign() {
            return this._verticalAlign;
        }
        set verticalAlign(value) {
            if (this._verticalAlign != value) {
                this._verticalAlign = value;
                this.updateLayout();
            }
        }
        get fill() {
            return this._fill;
        }
        set fill(value) {
            if (this._fill != value) {
                this._fill = value;
                this.updateLayout();
            }
        }
        get shrinkOnly() {
            return this._shrinkOnly;
        }
        set shrinkOnly(value) {
            if (this._shrinkOnly != value) {
                this._shrinkOnly = value;
                this.updateLayout();
            }
        }
        get autoSize() {
            return this._autoSize;
        }
        set autoSize(value) {
            if (this._autoSize != value) {
                this._autoSize = value;
                this.updateLayout();
            }
        }
        get playing() {
            return this._playing;
        }
        set playing(value) {
            if (this._playing != value) {
                this._playing = value;
                this.updateGear(5);
                this.onChange();
            }
        }
        get frame() {
            return this._frame;
        }
        set frame(value) {
            if (this._frame != value) {
                this._frame = value;
                this.updateGear(5);
                this.onChange();
            }
        }
        get animationName() {
            return this._animationName;
        }
        set animationName(value) {
            if (this._animationName != value) {
                this._animationName = value;
                this.onChange();
            }
        }
        get skinName() {
            return this._skinName;
        }
        set skinName(value) {
            if (this._skinName != value) {
                this._skinName = value;
                this.onChange();
            }
        }
        get loop() {
            return this._loop;
        }
        set loop(value) {
            if (this._loop != value) {
                this._loop = value;
                this.onChange();
            }
        }
        get color() {
            return this._color;
        }
        set color(value) {
            if (this._color != value) {
                this._color = value;
                this.updateGear(4);
                if (this._content)
                    fgui.ToolSet.setColorFilter(this._content, this._color);
            }
        }
        get content() {
            return this._content;
        }
        loadContent() {
            this.clearContent();
            if (!this._url)
                return;
            if (fgui.ToolSet.startsWith(this._url, "ui://"))
                this.loadFromPackage(this._url);
            else
                this.loadExternal();
        }
        loadFromPackage(itemURL) {
            this._contentItem = fgui.UIPackage.getItemByURL(itemURL);
            if (this._contentItem) {
                this._contentItem = this._contentItem.getBranch();
                this.sourceWidth = this._contentItem.width;
                this.sourceHeight = this._contentItem.height;
                this._contentItem = this._contentItem.getHighResolution();
                if (this._autoSize)
                    this.setSize(this.sourceWidth, this.sourceHeight);
                if (this._contentItem.type == fgui.PackageItemType.Spine || this._contentItem.type == fgui.PackageItemType.DragonBones)
                    this._contentItem.owner.getItemAssetAsync(this._contentItem, this.onLoaded.bind(this));
            }
        }
        onLoaded(err, item) {
            if (this._contentItem != item)
                return;
            if (err)
                console.warn(err);
            let templet = this._contentItem.templet;
            if (!templet)
                return;
            if (templet instanceof Laya.Templet)
                this.setSkeleton(templet.buildArmature(1), this._contentItem.skeletonAnchor);
            else {
                let obj = new Laya.SpineSkeleton();
                obj.templet = templet;
                this.setSkeleton(obj, this._contentItem.skeletonAnchor);
            }
        }
        setSkeleton(skeleton, anchor) {
            this.url = null;
            this._content = skeleton;
            this._container.addChild(this._content);
            this._content.pos(anchor.x, anchor.y);
            fgui.ToolSet.setColorFilter(this._content, this._color);
            this.onChange();
            this.updateLayout();
        }
        onChange() {
            if (!this._content)
                return;
            if (this._animationName) {
                if (this._playing)
                    this._content.play(this._animationName, this._loop);
                else
                    this._content.play(this._animationName, false, true, this._frame, this._frame);
            }
            else
                this._content.stop();
            if (this._skinName)
                this._content.showSkinByName(this._skinName);
            else
                this._content.showSkinByIndex(0);
        }
        loadExternal() {
        }
        updateLayout() {
            let cw = this.sourceWidth;
            let ch = this.sourceHeight;
            if (this._autoSize) {
                this._updatingLayout = true;
                if (cw == 0)
                    cw = 50;
                if (ch == 0)
                    ch = 30;
                this.setSize(cw, ch);
                this._updatingLayout = false;
                if (cw == this._width && ch == this._height) {
                    this._container.scale(1, 1);
                    this._container.pos(0, 0);
                    return;
                }
            }
            var sx = 1, sy = 1;
            if (this._fill != fgui.LoaderFillType.None) {
                sx = this.width / this.sourceWidth;
                sy = this.height / this.sourceHeight;
                if (sx != 1 || sy != 1) {
                    if (this._fill == fgui.LoaderFillType.ScaleMatchHeight)
                        sx = sy;
                    else if (this._fill == fgui.LoaderFillType.ScaleMatchWidth)
                        sy = sx;
                    else if (this._fill == fgui.LoaderFillType.Scale) {
                        if (sx > sy)
                            sx = sy;
                        else
                            sy = sx;
                    }
                    else if (this._fill == fgui.LoaderFillType.ScaleNoBorder) {
                        if (sx > sy)
                            sy = sx;
                        else
                            sx = sy;
                    }
                    if (this._shrinkOnly) {
                        if (sx > 1)
                            sx = 1;
                        if (sy > 1)
                            sy = 1;
                    }
                    cw = this.sourceWidth * sx;
                    ch = this.sourceHeight * sy;
                }
            }
            this._container.scale(sx, sy);
            var nx, ny;
            if (this._align == fgui.AlignType.Center)
                nx = Math.floor((this.width - cw) / 2);
            else if (this._align == fgui.AlignType.Right)
                nx = this.width - cw;
            else
                nx = 0;
            if (this._verticalAlign == fgui.VertAlignType.Middle)
                ny = Math.floor((this.height - ch) / 2);
            else if (this._verticalAlign == fgui.VertAlignType.Bottom)
                ny = this.height - ch;
            else
                ny = 0;
            this._container.pos(nx, ny);
        }
        clearContent() {
            this._contentItem = null;
            if (this._content) {
                this._container.removeChild(this._content);
                this._content.destroy();
                this._content = null;
            }
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            if (!this._updatingLayout)
                this.updateLayout();
        }
        handleGrayedChanged() {
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.color;
                case fgui.ObjectPropID.Playing:
                    return this.playing;
                case fgui.ObjectPropID.Frame:
                    return this.frame;
                case fgui.ObjectPropID.TimeScale:
                    return 1;
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.color = value;
                    break;
                case fgui.ObjectPropID.Playing:
                    this.playing = value;
                    break;
                case fgui.ObjectPropID.Frame:
                    this.frame = value;
                    break;
                case fgui.ObjectPropID.TimeScale:
                    break;
                case fgui.ObjectPropID.DeltaTime:
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            this._url = buffer.readS();
            this._align = buffer.readByte();
            this._verticalAlign = buffer.readByte();
            this._fill = buffer.readByte();
            this._shrinkOnly = buffer.readBool();
            this._autoSize = buffer.readBool();
            this._animationName = buffer.readS();
            this._skinName = buffer.readS();
            this._playing = buffer.readBool();
            this._frame = buffer.getInt32();
            this._loop = buffer.readBool();
            if (buffer.readBool())
                this.color = buffer.readColorS();
            if (this._url)
                this.loadContent();
        }
    }
    fgui.GLoader3D = GLoader3D;
})(fgui);

(function (fgui) {
    class GMovieClip extends fgui.GObject {
        constructor() {
            super();
        }
        get color() {
            return this._movieClip.color;
        }
        set color(value) {
            this._movieClip.color = value;
        }
        createDisplayObject() {
            this._displayObject = this._movieClip = new fgui.MovieClip();
            this._movieClip.mouseEnabled = false;
            this._displayObject["$owner"] = this;
        }
        get playing() {
            return this._movieClip.playing;
        }
        set playing(value) {
            if (this._movieClip.playing != value) {
                this._movieClip.playing = value;
                this.updateGear(5);
            }
        }
        get frame() {
            return this._movieClip.frame;
        }
        set frame(value) {
            if (this._movieClip.frame != value) {
                this._movieClip.frame = value;
                this.updateGear(5);
            }
        }
        get timeScale() {
            return this._movieClip.timeScale;
        }
        set timeScale(value) {
            this._movieClip.timeScale = value;
        }
        rewind() {
            this._movieClip.rewind();
        }
        syncStatus(anotherMc) {
            this._movieClip.syncStatus(anotherMc._movieClip);
        }
        advance(timeInMiniseconds) {
            this._movieClip.advance(timeInMiniseconds);
        }
        //从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
        setPlaySettings(start, end, times, endAt, endHandler) {
            this._movieClip.setPlaySettings(start, end, times, endAt, endHandler);
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.color;
                case fgui.ObjectPropID.Playing:
                    return this.playing;
                case fgui.ObjectPropID.Frame:
                    return this.frame;
                case fgui.ObjectPropID.TimeScale:
                    return this.timeScale;
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.color = value;
                    break;
                case fgui.ObjectPropID.Playing:
                    this.playing = value;
                    break;
                case fgui.ObjectPropID.Frame:
                    this.frame = value;
                    break;
                case fgui.ObjectPropID.TimeScale:
                    this.timeScale = value;
                    break;
                case fgui.ObjectPropID.DeltaTime:
                    this.advance(value);
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        constructFromResource() {
            var displayItem = this.packageItem.getBranch();
            this.sourceWidth = displayItem.width;
            this.sourceHeight = displayItem.height;
            this.initWidth = this.sourceWidth;
            this.initHeight = this.sourceHeight;
            this.setSize(this.sourceWidth, this.sourceHeight);
            displayItem = displayItem.getHighResolution();
            displayItem.load();
            this._movieClip.interval = displayItem.interval;
            this._movieClip.swing = displayItem.swing;
            this._movieClip.repeatDelay = displayItem.repeatDelay;
            this._movieClip.frames = displayItem.frames;
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            if (buffer.readBool())
                this.color = buffer.readColorS();
            buffer.readByte(); //flip
            this._movieClip.frame = buffer.getInt32();
            this._movieClip.playing = buffer.readBool();
        }
    }
    fgui.GMovieClip = GMovieClip;
})(fgui);

(function (fgui) {
    class GProgressBar extends fgui.GComponent {
        constructor() {
            super();
            this._min = 0;
            this._max = 0;
            this._value = 0;
            this._barMaxWidth = 0;
            this._barMaxHeight = 0;
            this._barMaxWidthDelta = 0;
            this._barMaxHeightDelta = 0;
            this._barStartX = 0;
            this._barStartY = 0;
            this._titleType = fgui.ProgressTitleType.Percent;
            this._value = 50;
            this._max = 100;
        }
        get titleType() {
            return this._titleType;
        }
        set titleType(value) {
            if (this._titleType != value) {
                this._titleType = value;
                this.update(value);
            }
        }
        get min() {
            return this._min;
        }
        set min(value) {
            if (this._min != value) {
                this._min = value;
                this.update(this._value);
            }
        }
        get max() {
            return this._max;
        }
        set max(value) {
            if (this._max != value) {
                this._max = value;
                this.update(this._value);
            }
        }
        get value() {
            return this._value;
        }
        set value(value) {
            if (this._value != value) {
                fgui.GTween.kill(this, false, this.update);
                this._value = value;
                this.update(value);
            }
        }
        tweenValue(value, duration) {
            var oldValule;
            var tweener = fgui.GTween.getTween(this, this.update);
            if (tweener) {
                oldValule = tweener.value.x;
                tweener.kill();
            }
            else
                oldValule = this._value;
            this._value = value;
            return fgui.GTween.to(oldValule, this._value, duration).setTarget(this, this.update).setEase(fgui.EaseType.Linear);
        }
        update(newValue) {
            var percent = fgui.ToolSet.clamp01((newValue - this._min) / (this._max - this._min));
            if (this._titleObject) {
                switch (this._titleType) {
                    case fgui.ProgressTitleType.Percent:
                        this._titleObject.text = Math.floor(percent * 100) + "%";
                        break;
                    case fgui.ProgressTitleType.ValueAndMax:
                        this._titleObject.text = Math.floor(newValue) + "/" + Math.floor(this._max);
                        break;
                    case fgui.ProgressTitleType.Value:
                        this._titleObject.text = "" + Math.floor(newValue);
                        break;
                    case fgui.ProgressTitleType.Max:
                        this._titleObject.text = "" + Math.floor(this._max);
                        break;
                }
            }
            var fullWidth = this.width - this._barMaxWidthDelta;
            var fullHeight = this.height - this._barMaxHeightDelta;
            if (!this._reverse) {
                if (this._barObjectH) {
                    if (!this.setFillAmount(this._barObjectH, percent))
                        this._barObjectH.width = Math.round(fullWidth * percent);
                }
                if (this._barObjectV) {
                    if (!this.setFillAmount(this._barObjectV, percent))
                        this._barObjectV.height = Math.round(fullHeight * percent);
                }
            }
            else {
                if (this._barObjectH) {
                    if (!this.setFillAmount(this._barObjectH, 1 - percent)) {
                        this._barObjectH.width = Math.round(fullWidth * percent);
                        this._barObjectH.x = this._barStartX + (fullWidth - this._barObjectH.width);
                    }
                }
                if (this._barObjectV) {
                    if (!this.setFillAmount(this._barObjectV, 1 - percent)) {
                        this._barObjectV.height = Math.round(fullHeight * percent);
                        this._barObjectV.y = this._barStartY + (fullHeight - this._barObjectV.height);
                    }
                }
            }
            if (this._aniObject)
                this._aniObject.setProp(fgui.ObjectPropID.Frame, Math.floor(percent * 100));
        }
        setFillAmount(bar, percent) {
            if (((bar instanceof fgui.GImage) || (bar instanceof fgui.GLoader)) && bar.fillMethod != fgui.FillMethod.None) {
                bar.fillAmount = percent;
                return true;
            }
            else
                return false;
        }
        constructExtension(buffer) {
            buffer.seek(0, 6);
            this._titleType = buffer.readByte();
            this._reverse = buffer.readBool();
            this._titleObject = this.getChild("title");
            this._barObjectH = this.getChild("bar");
            this._barObjectV = this.getChild("bar_v");
            this._aniObject = this.getChild("ani");
            if (this._barObjectH) {
                this._barMaxWidth = this._barObjectH.width;
                this._barMaxWidthDelta = this.width - this._barMaxWidth;
                this._barStartX = this._barObjectH.x;
            }
            if (this._barObjectV) {
                this._barMaxHeight = this._barObjectV.height;
                this._barMaxHeightDelta = this.height - this._barMaxHeight;
                this._barStartY = this._barObjectV.y;
            }
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            if (this._barObjectH)
                this._barMaxWidth = this.width - this._barMaxWidthDelta;
            if (this._barObjectV)
                this._barMaxHeight = this.height - this._barMaxHeightDelta;
            if (!this._underConstruct)
                this.update(this._value);
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            if (!buffer.seek(beginPos, 6)) {
                this.update(this._value);
                return;
            }
            if (buffer.readByte() != this.packageItem.objectType) {
                this.update(this._value);
                return;
            }
            this._value = buffer.getInt32();
            this._max = buffer.getInt32();
            if (buffer.version >= 2)
                this._min = buffer.getInt32();
            this.update(this._value);
        }
    }
    fgui.GProgressBar = GProgressBar;
})(fgui);
///<reference path="GObject.ts"/>

///<reference path="GObject.ts"/>
(function (fgui) {
    class GTextField extends fgui.GObject {
        constructor() {
            super();
            this._letterSpacing = 0;
            this._text = "";
            this._color = "#000000";
        }
        createDisplayObject() {
            this._displayObject = new Laya.Text();
            this._displayObject["$owner"] = this;
            this._displayObject.padding = labelPadding;
            this._displayObject.mouseEnabled = false;
            this._autoSize = fgui.AutoSizeType.Both;
            this._widthAutoSize = this._heightAutoSize = true;
            this._displayObject._onPostLayout = () => this.updateSize();
        }
        get displayObject() {
            return this._displayObject;
        }
        set text(value) {
            this._displayObject.text = value;
        }
        get text() {
            return this._displayObject.text;
        }
        get font() {
            return this._displayObject.font;
        }
        set font(value) {
            if (fgui.ToolSet.startsWith(value, "ui://"))
                fgui.UIPackage.getItemAssetByURL(value);
            this._displayObject.font = value;
        }
        get fontSize() {
            return this._displayObject.fontSize;
        }
        set fontSize(value) {
            this._displayObject.fontSize = value;
        }
        get color() {
            return this._color;
        }
        set color(value) {
            if (this._color != value) {
                this._color = value;
                this.updateGear(4);
                if (this.grayed)
                    this._displayObject.color = "#AAAAAA";
                else
                    this._displayObject.color = this._color;
            }
        }
        get align() {
            return this._displayObject.align;
        }
        set align(value) {
            this._displayObject.align = value;
        }
        get valign() {
            return this._displayObject.valign;
        }
        set valign(value) {
            this._displayObject.valign = value;
        }
        get leading() {
            return this._displayObject.leading;
        }
        set leading(value) {
            this._displayObject.leading = value;
        }
        get letterSpacing() {
            return this._letterSpacing;
        }
        set letterSpacing(value) {
            this._letterSpacing = value;
        }
        get bold() {
            return this._displayObject.bold;
        }
        set bold(value) {
            this._displayObject.bold = value;
        }
        get italic() {
            return this._displayObject.italic;
        }
        set italic(value) {
            this._displayObject.italic = value;
        }
        get underline() {
            return this._displayObject.underline;
        }
        set underline(value) {
            this._displayObject.underline = value;
        }
        get singleLine() {
            return this._singleLine;
        }
        set singleLine(value) {
            this._singleLine = value;
            this._displayObject.wordWrap = !this._widthAutoSize && !this._singleLine;
        }
        get stroke() {
            return this._displayObject.stroke;
        }
        set stroke(value) {
            this._displayObject.stroke = value;
        }
        get strokeColor() {
            return this._displayObject.strokeColor;
        }
        set strokeColor(value) {
            if (this._displayObject.strokeColor != value) {
                this._displayObject.strokeColor = value;
                this.updateGear(4);
            }
        }
        set ubbEnabled(value) {
            this._displayObject.ubb = value;
        }
        get ubbEnabled() {
            return this._displayObject.ubb;
        }
        get autoSize() {
            return this._autoSize;
        }
        set autoSize(value) {
            if (this._autoSize != value) {
                this._autoSize = value;
                this._widthAutoSize = this._autoSize == fgui.AutoSizeType.Both;
                this._heightAutoSize = this._autoSize == fgui.AutoSizeType.Both || this._autoSize == fgui.AutoSizeType.Height;
                this.updateAutoSize();
            }
        }
        updateAutoSize() {
            this._displayObject.wordWrap = !this._widthAutoSize && !this._singleLine;
            this._displayObject.overflow = this._autoSize == fgui.AutoSizeType.Shrink ? "shrink" : (this._autoSize == fgui.AutoSizeType.Ellipsis ? "ellipsis" : "visible");
            if (!this._underConstruct) {
                if (!this._heightAutoSize)
                    this._displayObject.size(this.width, this.height);
                else if (!this._widthAutoSize)
                    this._displayObject.width = this.width;
            }
        }
        get textWidth() {
            return this._displayObject.textWidth;
        }
        get templateVars() {
            return this._displayObject.templateVars;
        }
        set templateVars(value) {
            this._displayObject.templateVars = value;
        }
        setVar(name, value) {
            this._displayObject.setVar(name, value);
            return this;
        }
        flushVars() {
            //nothing here. auto flush
        }
        ensureSizeCorrect() {
            if (!this._underConstruct)
                this._displayObject.typeset();
        }
        updateSize() {
            if (this._widthAutoSize)
                this.setSize(this._displayObject.textWidth, this._displayObject.textHeight);
            else if (this._heightAutoSize)
                this.height = this._displayObject.textHeight;
        }
        handleSizeChanged() {
            this._displayObject.size(this._width, this._height);
        }
        handleGrayedChanged() {
            super.handleGrayedChanged();
            if (this.grayed)
                this._displayObject.color = "#AAAAAA";
            else
                this._displayObject.color = this._color;
        }
        getProp(index) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    return this.color;
                case fgui.ObjectPropID.OutlineColor:
                    return this.strokeColor;
                case fgui.ObjectPropID.FontSize:
                    return this.fontSize;
                default:
                    return super.getProp(index);
            }
        }
        setProp(index, value) {
            switch (index) {
                case fgui.ObjectPropID.Color:
                    this.color = value;
                    break;
                case fgui.ObjectPropID.OutlineColor:
                    this.strokeColor = value;
                    break;
                case fgui.ObjectPropID.FontSize:
                    this.fontSize = value;
                    break;
                default:
                    super.setProp(index, value);
                    break;
            }
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 5);
            var iv;
            this.font = buffer.readS();
            this.fontSize = buffer.getInt16();
            this.color = buffer.readColorS();
            iv = buffer.readByte();
            this.align = iv == 0 ? "left" : (iv == 1 ? "center" : "right");
            iv = buffer.readByte();
            this.valign = iv == 0 ? "top" : (iv == 1 ? "middle" : "bottom");
            this.leading = buffer.getInt16();
            this.letterSpacing = buffer.getInt16();
            this.ubbEnabled = buffer.readBool();
            this.autoSize = buffer.readByte();
            this.underline = buffer.readBool();
            this.italic = buffer.readBool();
            this.bold = buffer.readBool();
            this.singleLine = buffer.readBool();
            if (buffer.readBool()) {
                this.strokeColor = buffer.readColorS();
                this.stroke = buffer.getFloat32() + 1;
            }
            if (buffer.readBool()) //shadow
                buffer.skip(12);
            if (buffer.readBool())
                this._displayObject.templateVars = {};
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            buffer.seek(beginPos, 6);
            var str = buffer.readS();
            if (str != null)
                this.text = str;
        }
    }
    fgui.GTextField = GTextField;
})(fgui);
const labelPadding = [2, 2, 2, 2];
///<reference path="GTextField.ts"/>

///<reference path="GTextField.ts"/>
(function (fgui) {
    class GRichTextField extends fgui.GTextField {
        constructor() {
            super();
            this._displayObject.html = true;
            this._displayObject.mouseEnabled = true;
        }
    }
    fgui.GRichTextField = GRichTextField;
    class GHtmlImage extends Laya.HtmlImage {
        loadTexture(src) {
            if (src.startsWith("ui://")) {
                let tex = fgui.UIPackage.getItemAssetByURL(src);
                if (tex instanceof Laya.Texture) {
                    let width = this.element.getAttr('width');
                    let height = this.element.getAttr('height');
                    if (width != null) {
                        if (width.toString().indexOf('%') >= 0) {
                            width = tex.sourceWidth * parseInt(width) / 100;
                        }
                        else {
                            width = parseInt(width);
                        }
                    }
                    else {
                        width = tex.sourceWidth;
                    }
                    if (height != null) {
                        if (height.toString().indexOf('%') >= 0) {
                            height = tex.sourceHeight * parseInt(height) / 100;
                        }
                        else {
                            height = parseInt(height);
                        }
                    }
                    else {
                        height = tex.sourceHeight;
                    }
                    this.obj.width = width;
                    this.obj.height = height;
                    this.obj.texture = tex;
                }
            }
            else {
                super.loadTexture(src);
            }
        }
    }
    fgui.GHtmlImage = GHtmlImage;
    Laya.HtmlParser.classMap[Laya.HtmlElementType.Image] = GHtmlImage;
})(fgui);

(function (fgui) {
    class GRoot extends fgui.GComponent {
        static get inst() {
            if (!GRoot._inst)
                new GRoot();
            return GRoot._inst;
        }
        constructor() {
            super();
            if (!GRoot._inst)
                GRoot._inst = this;
            this.opaque = false;
            this._popupStack = [];
            this._justClosedPopups = [];
            this.displayObject.once(Laya.Event.DISPLAY, this, this.__addedToStage);
        }
        showWindow(win) {
            this.addChild(win);
            win.requestFocus();
            if (win.x > this.width)
                win.x = this.width - win.width;
            else if (win.x + win.width < 0)
                win.x = 0;
            if (win.y > this.height)
                win.y = this.height - win.height;
            else if (win.y + win.height < 0)
                win.y = 0;
            this.adjustModalLayer();
        }
        hideWindow(win) {
            win.hide();
        }
        hideWindowImmediately(win) {
            if (win.parent == this)
                this.removeChild(win);
            this.adjustModalLayer();
        }
        bringToFront(win) {
            var cnt = this.numChildren;
            var i;
            if (this._modalLayer.parent && !win.modal)
                i = this.getChildIndex(this._modalLayer) - 1;
            else
                i = cnt - 1;
            for (; i >= 0; i--) {
                var g = this.getChildAt(i);
                if (g == win)
                    return;
                if (g instanceof fgui.Window)
                    break;
            }
            if (i >= 0)
                this.setChildIndex(win, i);
        }
        showModalWait(msg) {
            if (fgui.UIConfig.globalModalWaiting != null) {
                if (this._modalWaitPane == null)
                    this._modalWaitPane = fgui.UIPackage.createObjectFromURL(fgui.UIConfig.globalModalWaiting);
                this._modalWaitPane.setSize(this.width, this.height);
                this._modalWaitPane.addRelation(this, fgui.RelationType.Size);
                this.addChild(this._modalWaitPane);
                this._modalWaitPane.text = msg;
            }
        }
        closeModalWait() {
            if (this._modalWaitPane && this._modalWaitPane.parent)
                this.removeChild(this._modalWaitPane);
        }
        closeAllExceptModals() {
            var arr = this._children.slice();
            var cnt = arr.length;
            for (var i = 0; i < cnt; i++) {
                var g = arr[i];
                if ((g instanceof fgui.Window) && !g.modal)
                    g.hide();
            }
        }
        closeAllWindows() {
            var arr = this._children.slice();
            var cnt = arr.length;
            for (var i = 0; i < cnt; i++) {
                var g = arr[i];
                if (g instanceof fgui.Window)
                    g.hide();
            }
        }
        getTopWindow() {
            var cnt = this.numChildren;
            for (var i = cnt - 1; i >= 0; i--) {
                var g = this.getChildAt(i);
                if (g instanceof fgui.Window) {
                    return g;
                }
            }
            return null;
        }
        get modalLayer() {
            return this._modalLayer;
        }
        get hasModalWindow() {
            return this._modalLayer.parent != null;
        }
        get modalWaiting() {
            return this._modalWaitPane && this._modalWaitPane.inContainer;
        }
        showPopup(popup, target, dir) {
            if (this._popupStack.length > 0) {
                var k = this._popupStack.indexOf(popup);
                if (k != -1) {
                    for (var i = this._popupStack.length - 1; i >= k; i--)
                        this.removeChild(this._popupStack.pop());
                }
            }
            this._popupStack.push(popup);
            if (target) {
                var p = target;
                while (p) {
                    if (p.parent == this) {
                        if (popup.sortingOrder < p.sortingOrder) {
                            popup.sortingOrder = p.sortingOrder;
                        }
                        break;
                    }
                    p = p.parent;
                }
            }
            this.addChild(popup);
            this.adjustModalLayer();
            var pos;
            var sizeW = 0, sizeH = 0;
            if (target) {
                pos = target.localToGlobal();
                sizeW = target.width;
                sizeH = target.height;
            }
            else {
                pos = this.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY);
            }
            var xx, yy;
            xx = pos.x;
            if (xx + popup.width > this.width)
                xx = xx + sizeW - popup.width;
            yy = pos.y + sizeH;
            if (((dir === undefined || dir === fgui.PopupDirection.Auto) && pos.y + popup.height > this.height)
                || dir === false || dir === fgui.PopupDirection.Up) {
                yy = pos.y - popup.height - 1;
                if (yy < 0) {
                    yy = 0;
                    xx += sizeW / 2;
                }
            }
            popup.x = xx;
            popup.y = yy;
        }
        togglePopup(popup, target, dir) {
            if (this._justClosedPopups.indexOf(popup) != -1)
                return;
            this.showPopup(popup, target, dir);
        }
        hidePopup(popup) {
            if (popup) {
                var k = this._popupStack.indexOf(popup);
                if (k != -1) {
                    for (var i = this._popupStack.length - 1; i >= k; i--)
                        this.closePopup(this._popupStack.pop());
                }
            }
            else {
                var cnt = this._popupStack.length;
                for (i = cnt - 1; i >= 0; i--)
                    this.closePopup(this._popupStack[i]);
                this._popupStack.length = 0;
            }
        }
        get hasAnyPopup() {
            return this._popupStack.length != 0;
        }
        closePopup(target) {
            if (target.parent) {
                if (target instanceof fgui.Window)
                    target.hide();
                else
                    this.removeChild(target);
            }
        }
        showTooltips(msg) {
            if (this._defaultTooltipWin == null) {
                var resourceURL = fgui.UIConfig.tooltipsWin;
                if (!resourceURL) {
                    Laya.Log.print("UIConfig.tooltipsWin not defined");
                    return;
                }
                this._defaultTooltipWin = fgui.UIPackage.createObjectFromURL(resourceURL);
            }
            this._defaultTooltipWin.text = msg;
            this.showTooltipsWin(this._defaultTooltipWin);
        }
        showTooltipsWin(tooltipWin, position) {
            this.hideTooltips();
            this._tooltipWin = tooltipWin;
            var xx = 0;
            var yy = 0;
            if (position == null) {
                xx = Laya.stage.mouseX + 10;
                yy = Laya.stage.mouseY + 20;
            }
            else {
                xx = position.x;
                yy = position.y;
            }
            var pt = this.globalToLocal(xx, yy);
            xx = pt.x;
            yy = pt.y;
            if (xx + this._tooltipWin.width > this.width) {
                xx = xx - this._tooltipWin.width - 1;
                if (xx < 0)
                    xx = 10;
            }
            if (yy + this._tooltipWin.height > this.height) {
                yy = yy - this._tooltipWin.height - 1;
                if (xx - this._tooltipWin.width - 1 > 0)
                    xx = xx - this._tooltipWin.width - 1;
                if (yy < 0)
                    yy = 10;
            }
            this._tooltipWin.x = xx;
            this._tooltipWin.y = yy;
            this.addChild(this._tooltipWin);
        }
        hideTooltips() {
            if (this._tooltipWin) {
                if (this._tooltipWin.parent)
                    this.removeChild(this._tooltipWin);
                this._tooltipWin = null;
            }
        }
        get focus() {
            return null;
        }
        set focus(value) {
            this.setFocus(value);
        }
        setFocus(value) {
        }
        get volumeScale() {
            return Laya.SoundManager.soundVolume;
        }
        set volumeScale(value) {
            Laya.SoundManager.soundVolume = value;
        }
        playOneShotSound(url, volumeScale) {
            if (fgui.ToolSet.startsWith(url, "ui://"))
                return;
            Laya.SoundManager.playSound(url);
        }
        adjustModalLayer() {
            var cnt = this.numChildren;
            if (this._modalWaitPane != null && this._modalWaitPane.parent != null)
                this.setChildIndex(this._modalWaitPane, cnt - 1);
            for (var i = cnt - 1; i >= 0; i--) {
                var g = this.getChildAt(i);
                if ((g instanceof fgui.Window) && g.modal) {
                    if (this._modalLayer.parent == null)
                        this.addChildAt(this._modalLayer, i);
                    else
                        this.setChildIndexBefore(this._modalLayer, i);
                    return;
                }
            }
            if (this._modalLayer.parent)
                this.removeChild(this._modalLayer);
        }
        __addedToStage() {
            Laya.stage.on(Laya.Event.MOUSE_DOWN, this, this.__stageMouseDown);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__stageMouseUp);
            this._modalLayer = new fgui.GGraph();
            this._modalLayer.setSize(this.width, this.height);
            this._modalLayer.drawRect(0, null, fgui.UIConfig.modalLayerColor);
            this._modalLayer.addRelation(this, fgui.RelationType.Size);
            this.displayObject.stage.on(Laya.Event.RESIZE, this, this.__winResize);
            this.__winResize();
        }
        checkPopups(clickTarget) {
            if (this._checkPopups)
                return;
            this._checkPopups = true;
            this._justClosedPopups.length = 0;
            if (this._popupStack.length > 0) {
                var mc = clickTarget;
                while (mc != this.displayObject.stage && mc) {
                    if (mc["$owner"]) {
                        var pindex = this._popupStack.indexOf(mc["$owner"]);
                        if (pindex != -1) {
                            for (var i = this._popupStack.length - 1; i > pindex; i--) {
                                var popup = this._popupStack.pop();
                                this.closePopup(popup);
                                this._justClosedPopups.push(popup);
                            }
                            return;
                        }
                    }
                    mc = mc.parent;
                }
                var cnt = this._popupStack.length;
                for (i = cnt - 1; i >= 0; i--) {
                    popup = this._popupStack[i];
                    this.closePopup(popup);
                    this._justClosedPopups.push(popup);
                }
                this._popupStack.length = 0;
            }
        }
        __stageMouseDown(evt) {
            if (this._tooltipWin)
                this.hideTooltips();
            this.checkPopups(evt.target);
        }
        __stageMouseUp() {
            this._checkPopups = false;
        }
        __winResize() {
            this.setSize(Laya.stage.width, Laya.stage.height);
            this.updateContentScaleLevel();
        }
        updateContentScaleLevel() {
            var mat = Laya.stage._canvasTransform;
            var ss = Math.max(mat.getScaleX(), mat.getScaleY());
            if (ss >= 3.5)
                GRoot.contentScaleLevel = 3; //x4
            else if (ss >= 2.5)
                GRoot.contentScaleLevel = 2; //x3
            else if (ss >= 1.5)
                GRoot.contentScaleLevel = 1; //x2
            else
                GRoot.contentScaleLevel = 0;
        }
    }
    GRoot.contentScaleLevel = 0;
    fgui.GRoot = GRoot;
})(fgui);

(function (fgui) {
    class GScrollBar extends fgui.GComponent {
        constructor() {
            super();
            this._dragOffset = new Laya.Point();
            this._scrollPerc = 0;
        }
        setScrollPane(target, vertical) {
            this._target = target;
            this._vertical = vertical;
        }
        setDisplayPerc(value) {
            if (this._vertical) {
                if (!this._fixedGripSize)
                    this._grip.height = Math.floor(value * this._bar.height);
                this._grip.y = this._bar.y + (this._bar.height - this._grip.height) * this._scrollPerc;
            }
            else {
                if (!this._fixedGripSize)
                    this._grip.width = Math.floor(value * this._bar.width);
                this._grip.x = this._bar.x + (this._bar.width - this._grip.width) * this._scrollPerc;
            }
            this._grip.visible = value != 0 && value != 1;
        }
        setScrollPerc(val) {
            this._scrollPerc = val;
            if (this._vertical)
                this._grip.y = this._bar.y + (this._bar.height - this._grip.height) * this._scrollPerc;
            else
                this._grip.x = this._bar.x + (this._bar.width - this._grip.width) * this._scrollPerc;
        }
        get minSize() {
            if (this._vertical)
                return (this._arrowButton1 ? this._arrowButton1.height : 0) + (this._arrowButton2 ? this._arrowButton2.height : 0);
            else
                return (this._arrowButton1 ? this._arrowButton1.width : 0) + (this._arrowButton2 ? this._arrowButton2.width : 0);
        }
        get gripDragging() {
            return this._gripDragging;
        }
        constructExtension(buffer) {
            buffer.seek(0, 6);
            this._fixedGripSize = buffer.readBool();
            this._grip = this.getChild("grip");
            if (!this._grip) {
                Laya.Log.print("需要定义grip");
                return;
            }
            this._bar = this.getChild("bar");
            if (!this._bar) {
                Laya.Log.print("需要定义bar");
                return;
            }
            this._arrowButton1 = this.getChild("arrow1");
            this._arrowButton2 = this.getChild("arrow2");
            this._grip.on(Laya.Event.MOUSE_DOWN, this, this.__gripMouseDown);
            if (this._arrowButton1)
                this._arrowButton1.on(Laya.Event.MOUSE_DOWN, this, this.__arrowButton1Click);
            if (this._arrowButton2)
                this._arrowButton2.on(Laya.Event.MOUSE_DOWN, this, this.__arrowButton2Click);
            this.on(Laya.Event.MOUSE_DOWN, this, this.__barMouseDown);
        }
        __gripMouseDown(evt) {
            evt.stopPropagation();
            this._gripDragging = true;
            this._target.updateScrollBarVisible();
            Laya.stage.on(Laya.Event.MOUSE_MOVE, this, this.__gripMouseMove);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__gripMouseUp);
            this.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY, this._dragOffset);
            this._dragOffset.x -= this._grip.x;
            this._dragOffset.y -= this._grip.y;
        }
        __gripMouseMove() {
            if (!this.onStage)
                return;
            var pt = this.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY, s_vec2);
            if (this._vertical) {
                var curY = pt.y - this._dragOffset.y;
                this._target.setPercY((curY - this._bar.y) / (this._bar.height - this._grip.height), false);
            }
            else {
                var curX = pt.x - this._dragOffset.x;
                this._target.setPercX((curX - this._bar.x) / (this._bar.width - this._grip.width), false);
            }
        }
        __gripMouseUp(evt) {
            if (!this.onStage)
                return;
            Laya.stage.off(Laya.Event.MOUSE_MOVE, this, this.__gripMouseMove);
            Laya.stage.off(Laya.Event.MOUSE_UP, this, this.__gripMouseUp);
            this._gripDragging = false;
            this._target.updateScrollBarVisible();
        }
        __arrowButton1Click(evt) {
            evt.stopPropagation();
            if (this._vertical)
                this._target.scrollUp();
            else
                this._target.scrollLeft();
        }
        __arrowButton2Click(evt) {
            evt.stopPropagation();
            if (this._vertical)
                this._target.scrollDown();
            else
                this._target.scrollRight();
        }
        __barMouseDown(evt) {
            var pt = this._grip.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY, s_vec2);
            if (this._vertical) {
                if (pt.y < 0)
                    this._target.scrollUp(4);
                else
                    this._target.scrollDown(4);
            }
            else {
                if (pt.x < 0)
                    this._target.scrollLeft(4);
                else
                    this._target.scrollRight(4);
            }
        }
    }
    fgui.GScrollBar = GScrollBar;
    var s_vec2 = new Laya.Point();
})(fgui);

(function (fgui) {
    class GSlider extends fgui.GComponent {
        constructor() {
            super();
            this._min = 0;
            this._max = 0;
            this._value = 0;
            this._barMaxWidth = 0;
            this._barMaxHeight = 0;
            this._barMaxWidthDelta = 0;
            this._barMaxHeightDelta = 0;
            this._clickPercent = 0;
            this._barStartX = 0;
            this._barStartY = 0;
            this.changeOnClick = true;
            /**是否可拖动开关**/
            this.canDrag = true;
            this._titleType = fgui.ProgressTitleType.Percent;
            this._value = 50;
            this._max = 100;
            this._clickPos = new Laya.Point();
        }
        get titleType() {
            return this._titleType;
        }
        set titleType(value) {
            this._titleType = value;
        }
        get wholeNumbers() {
            return this._wholeNumbers;
        }
        set wholeNumbers(value) {
            if (this._wholeNumbers != value) {
                this._wholeNumbers = value;
                this.update();
            }
        }
        get min() {
            return this._min;
        }
        set min(value) {
            if (this._min != value) {
                this._min = value;
                this.update();
            }
        }
        get max() {
            return this._max;
        }
        set max(value) {
            if (this._max != value) {
                this._max = value;
                this.update();
            }
        }
        get value() {
            return this._value;
        }
        set value(value) {
            if (this._value != value) {
                this._value = value;
                this.update();
            }
        }
        update() {
            this.updateWithPercent((this._value - this._min) / (this._max - this._min));
        }
        updateWithPercent(percent, evt) {
            percent = fgui.ToolSet.clamp01(percent);
            if (evt) {
                var newValue = fgui.ToolSet.clamp(this._min + (this._max - this._min) * percent, this._min, this._max);
                if (this._wholeNumbers) {
                    newValue = Math.round(newValue);
                    percent = fgui.ToolSet.clamp01((newValue - this._min) / (this._max - this._min));
                }
                if (newValue != this._value) {
                    this._value = newValue;
                    fgui.Events.dispatch(fgui.Events.STATE_CHANGED, this.displayObject, evt);
                }
            }
            if (this._titleObject) {
                switch (this._titleType) {
                    case fgui.ProgressTitleType.Percent:
                        this._titleObject.text = Math.floor(percent * 100) + "%";
                        break;
                    case fgui.ProgressTitleType.ValueAndMax:
                        this._titleObject.text = this._value + "/" + this._max;
                        break;
                    case fgui.ProgressTitleType.Value:
                        this._titleObject.text = "" + this._value;
                        break;
                    case fgui.ProgressTitleType.Max:
                        this._titleObject.text = "" + this._max;
                        break;
                }
            }
            var fullWidth = this.width - this._barMaxWidthDelta;
            var fullHeight = this.height - this._barMaxHeightDelta;
            if (!this._reverse) {
                if (this._barObjectH)
                    this._barObjectH.width = Math.round(fullWidth * percent);
                if (this._barObjectV)
                    this._barObjectV.height = Math.round(fullHeight * percent);
            }
            else {
                if (this._barObjectH) {
                    this._barObjectH.width = Math.round(fullWidth * percent);
                    this._barObjectH.x = this._barStartX + (fullWidth - this._barObjectH.width);
                }
                if (this._barObjectV) {
                    this._barObjectV.height = Math.round(fullHeight * percent);
                    this._barObjectV.y = this._barStartY + (fullHeight - this._barObjectV.height);
                }
            }
        }
        constructExtension(buffer) {
            buffer.seek(0, 6);
            this._titleType = buffer.readByte();
            this._reverse = buffer.readBool();
            if (buffer.version >= 2) {
                this._wholeNumbers = buffer.readBool();
                this.changeOnClick = buffer.readBool();
            }
            this._titleObject = this.getChild("title");
            this._barObjectH = this.getChild("bar");
            this._barObjectV = this.getChild("bar_v");
            this._gripObject = this.getChild("grip");
            if (this._barObjectH) {
                this._barMaxWidth = this._barObjectH.width;
                this._barMaxWidthDelta = this.width - this._barMaxWidth;
                this._barStartX = this._barObjectH.x;
            }
            if (this._barObjectV) {
                this._barMaxHeight = this._barObjectV.height;
                this._barMaxHeightDelta = this.height - this._barMaxHeight;
                this._barStartY = this._barObjectV.y;
            }
            if (this._gripObject) {
                this._gripObject.on(Laya.Event.MOUSE_DOWN, this, this.__gripMouseDown);
            }
            this.displayObject.on(Laya.Event.MOUSE_DOWN, this, this.__barMouseDown);
        }
        handleSizeChanged() {
            super.handleSizeChanged();
            if (this._barObjectH)
                this._barMaxWidth = this.width - this._barMaxWidthDelta;
            if (this._barObjectV)
                this._barMaxHeight = this.height - this._barMaxHeightDelta;
            if (!this._underConstruct)
                this.update();
        }
        setup_afterAdd(buffer, beginPos) {
            super.setup_afterAdd(buffer, beginPos);
            if (!buffer.seek(beginPos, 6)) {
                this.update();
                return;
            }
            if (buffer.readByte() != this.packageItem.objectType) {
                this.update();
                return;
            }
            this._value = buffer.getInt32();
            this._max = buffer.getInt32();
            if (buffer.version >= 2)
                this._min = buffer.getInt32();
            this.update();
        }
        __gripMouseDown(evt) {
            this.canDrag = true;
            evt.stopPropagation();
            this._clickPos = this.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY);
            this._clickPercent = fgui.ToolSet.clamp01((this._value - this._min) / (this._max - this._min));
            Laya.stage.on(Laya.Event.MOUSE_MOVE, this, this.__gripMouseMove);
            Laya.stage.on(Laya.Event.MOUSE_UP, this, this.__gripMouseUp);
        }
        __gripMouseMove(evt) {
            if (!this.canDrag) {
                return;
            }
            var pt = this.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY, s_vec2);
            var deltaX = pt.x - this._clickPos.x;
            var deltaY = pt.y - this._clickPos.y;
            if (this._reverse) {
                deltaX = -deltaX;
                deltaY = -deltaY;
            }
            var percent;
            if (this._barObjectH)
                percent = this._clickPercent + deltaX / this._barMaxWidth;
            else
                percent = this._clickPercent + deltaY / this._barMaxHeight;
            this.updateWithPercent(percent, evt);
        }
        __gripMouseUp(evt) {
            Laya.stage.off(Laya.Event.MOUSE_MOVE, this, this.__gripMouseMove);
            Laya.stage.off(Laya.Event.MOUSE_UP, this, this.__gripMouseUp);
        }
        __barMouseDown(evt) {
            if (!this.changeOnClick)
                return;
            var pt = this._gripObject.globalToLocal(evt.stageX, evt.stageY, s_vec2);
            var percent = fgui.ToolSet.clamp01((this._value - this._min) / (this._max - this._min));
            var delta;
            if (this._barObjectH)
                delta = pt.x / this._barMaxWidth;
            if (this._barObjectV)
                delta = pt.y / this._barMaxHeight;
            if (this._reverse)
                percent -= delta;
            else
                percent += delta;
            this.updateWithPercent(percent, evt);
        }
    }
    fgui.GSlider = GSlider;
    var s_vec2 = new Laya.Point();
})(fgui);
///<reference path="GTextField.ts"/>

///<reference path="GTextField.ts"/>
(function (fgui) {
    class GTextInput extends fgui.GTextField {
        constructor() {
            super();
        }
        createDisplayObject() {
            this._displayObject = new Laya.Input();
            this._displayObject["$owner"] = this;
            this.on(Laya.Event.BLUR, this, this.__onBlur);
            this.on(Laya.Event.FOCUS, this, this.__onFocus);
        }
        __onBlur() {
            super.updateSize();
            this._displayObject.event(fgui.Events.BLUR);
        }
        __onFocus() {
            this._displayObject.event(fgui.Events.FOCUS);
        }
        get nativeInput() {
            return this._displayObject;
        }
        get password() {
            return this._displayObject.type == "password";
        }
        set password(value) {
            if (value)
                this._displayObject.type = "password";
            else
                this._displayObject.type = "text";
        }
        get keyboardType() {
            return this._displayObject.type;
        }
        set keyboardType(value) {
            this._displayObject.type = value;
        }
        set editable(value) {
            this._displayObject.editable = value;
        }
        get editable() {
            return this._displayObject.editable;
        }
        set maxLength(value) {
            this._displayObject.maxChars = value;
        }
        get maxLength() {
            return this._displayObject.maxChars;
        }
        set promptText(value) {
            var str = fgui.UBBParser.defaultParser.parse(value, true);
            this._displayObject.prompt = str;
            if (fgui.UBBParser.defaultParser.lastColor)
                this._displayObject.promptColor = fgui.UBBParser.defaultParser.lastColor;
        }
        get promptText() {
            return this._displayObject.prompt;
        }
        set restrict(value) {
            this._displayObject.restrict = value;
        }
        get restrict() {
            return this._displayObject.restrict;
        }
        requestFocus() {
            this._displayObject.focus = true;
            super.requestFocus();
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 4);
            var str = buffer.readS();
            if (str != null)
                this.promptText = str;
            str = buffer.readS();
            if (str != null)
                this._displayObject.restrict = str;
            var iv = buffer.getInt32();
            if (iv != 0)
                this._displayObject.maxChars = iv;
            iv = buffer.getInt32();
            if (iv != 0) {
                if (iv == 4)
                    this.keyboardType = Laya.Input.TYPE_NUMBER;
                else if (iv == 3)
                    this.keyboardType = Laya.Input.TYPE_URL;
            }
            if (buffer.readBool())
                this.password = true;
        }
    }
    fgui.GTextInput = GTextInput;
})(fgui);

(function (fgui) {
    class GTree extends fgui.GList {
        constructor() {
            super();
            this._indent = 15;
            this._rootNode = new fgui.GTreeNode(true);
            this._rootNode._setTree(this);
            this._rootNode.expanded = true;
        }
        get rootNode() {
            return this._rootNode;
        }
        get indent() {
            return this._indent;
        }
        set indent(value) {
            this._indent = value;
        }
        get clickToExpand() {
            return this._clickToExpand;
        }
        set clickToExpand(value) {
            this._clickToExpand = value;
        }
        getSelectedNode() {
            if (this.selectedIndex != -1)
                return this.getChildAt(this.selectedIndex)._treeNode;
            else
                return null;
        }
        getSelectedNodes(result) {
            if (!result)
                result = new Array();
            s_list.length = 0;
            super.getSelection(s_list);
            var cnt = s_list.length;
            var ret = new Array();
            for (var i = 0; i < cnt; i++) {
                var node = this.getChildAt(s_list[i])._treeNode;
                ret.push(node);
            }
            return ret;
        }
        selectNode(node, scrollItToView) {
            var parentNode = node.parent;
            while (parentNode && parentNode != this._rootNode) {
                parentNode.expanded = true;
                parentNode = parentNode.parent;
            }
            if (!node._cell)
                return;
            this.addSelection(this.getChildIndex(node._cell), scrollItToView);
        }
        unselectNode(node) {
            if (!node._cell)
                return;
            this.removeSelection(this.getChildIndex(node._cell));
        }
        expandAll(folderNode) {
            if (!folderNode)
                folderNode = this._rootNode;
            folderNode.expanded = true;
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++) {
                var node = folderNode.getChildAt(i);
                if (node.isFolder)
                    this.expandAll(node);
            }
        }
        collapseAll(folderNode) {
            if (!folderNode)
                folderNode = this._rootNode;
            if (folderNode != this._rootNode)
                folderNode.expanded = false;
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++) {
                var node = folderNode.getChildAt(i);
                if (node.isFolder)
                    this.collapseAll(node);
            }
        }
        createCell(node) {
            var child = this.getFromPool(node._resURL ? node._resURL : this.defaultItem);
            if (!child)
                throw new Error("cannot create tree node object.");
            child._treeNode = node;
            node._cell = child;
            var indentObj = child.getChild("indent");
            if (indentObj)
                indentObj.width = (node.level - 1) * this._indent;
            var cc;
            cc = child.getController("expanded");
            if (cc) {
                cc.on(fgui.Events.STATE_CHANGED, this, this.__expandedStateChanged);
                cc.selectedIndex = node.expanded ? 1 : 0;
            }
            cc = child.getController("leaf");
            if (cc)
                cc.selectedIndex = node.isFolder ? 0 : 1;
            if (node.isFolder)
                child.on(Laya.Event.MOUSE_DOWN, this, this.__cellMouseDown);
            if (typeof this.treeNodeRender === 'function')
                this.treeNodeRender(node, child);
            else if (this.treeNodeRender)
                this.treeNodeRender.runWith([node, child]);
        }
        _afterInserted(node) {
            if (!node._cell)
                this.createCell(node);
            var index = this.getInsertIndexForNode(node);
            this.addChildAt(node._cell, index);
            if (typeof this.treeNodeRender === 'function')
                this.treeNodeRender(node, node._cell);
            else if (this.treeNodeRender)
                this.treeNodeRender.runWith([node, node._cell]);
            if (node.isFolder && node.expanded)
                this.checkChildren(node, index);
        }
        getInsertIndexForNode(node) {
            var prevNode = node.getPrevSibling();
            if (!prevNode)
                prevNode = node.parent;
            var insertIndex = this.getChildIndex(prevNode._cell) + 1;
            var myLevel = node.level;
            var cnt = this.numChildren;
            for (var i = insertIndex; i < cnt; i++) {
                var testNode = this.getChildAt(i)._treeNode;
                if (testNode.level <= myLevel)
                    break;
                insertIndex++;
            }
            return insertIndex;
        }
        _afterRemoved(node) {
            this.removeNode(node);
        }
        _afterExpanded(node) {
            if (node == this._rootNode) {
                this.checkChildren(this._rootNode, 0);
                return;
            }
            if (typeof this.treeNodeWillExpand === 'function')
                this.treeNodeWillExpand(node, true);
            else if (this.treeNodeWillExpand)
                this.treeNodeWillExpand.runWith([node, true]);
            if (!node._cell)
                return;
            if (typeof this.treeNodeRender === 'function')
                this.treeNodeRender(node, node._cell);
            else if (this.treeNodeRender)
                this.treeNodeRender.runWith([node, node._cell]);
            var cc = node._cell.getController("expanded");
            if (cc)
                cc.selectedIndex = 1;
            if (node._cell.parent)
                this.checkChildren(node, this.getChildIndex(node._cell));
        }
        _afterCollapsed(node) {
            if (node == this._rootNode) {
                this.checkChildren(this._rootNode, 0);
                return;
            }
            if (typeof this.treeNodeWillExpand === 'function')
                this.treeNodeWillExpand(node, false);
            else if (this.treeNodeWillExpand)
                this.treeNodeWillExpand.runWith([node, false]);
            if (!node._cell)
                return;
            if (typeof this.treeNodeRender === 'function')
                this.treeNodeRender(node, node._cell);
            else if (this.treeNodeRender)
                this.treeNodeRender.runWith([node, node._cell]);
            var cc = node._cell.getController("expanded");
            if (cc)
                cc.selectedIndex = 0;
            if (node._cell.parent)
                this.hideFolderNode(node);
        }
        _afterMoved(node) {
            var startIndex = this.getChildIndex(node._cell);
            var endIndex;
            if (node.isFolder)
                endIndex = this.getFolderEndIndex(startIndex, node.level);
            else
                endIndex = startIndex + 1;
            var insertIndex = this.getInsertIndexForNode(node);
            var i;
            var cnt = endIndex - startIndex;
            var obj;
            if (insertIndex < startIndex) {
                for (i = 0; i < cnt; i++) {
                    obj = this.getChildAt(startIndex + i);
                    this.setChildIndex(obj, insertIndex + i);
                }
            }
            else {
                for (i = 0; i < cnt; i++) {
                    obj = this.getChildAt(startIndex);
                    this.setChildIndex(obj, insertIndex);
                }
            }
        }
        getFolderEndIndex(startIndex, level) {
            var cnt = this.numChildren;
            for (var i = startIndex + 1; i < cnt; i++) {
                var node = this.getChildAt(i)._treeNode;
                if (node.level <= level)
                    return i;
            }
            return cnt;
        }
        checkChildren(folderNode, index) {
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++) {
                index++;
                var node = folderNode.getChildAt(i);
                if (!node._cell)
                    this.createCell(node);
                if (!node._cell.parent)
                    this.addChildAt(node._cell, index);
                if (node.isFolder && node.expanded)
                    index = this.checkChildren(node, index);
            }
            return index;
        }
        hideFolderNode(folderNode) {
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++) {
                var node = folderNode.getChildAt(i);
                if (node._cell)
                    this.removeChild(node._cell);
                if (node.isFolder && node.expanded)
                    this.hideFolderNode(node);
            }
        }
        removeNode(node) {
            if (node._cell) {
                if (node._cell.parent)
                    this.removeChild(node._cell);
                this.returnToPool(node._cell);
                node._cell._treeNode = null;
                node._cell = null;
            }
            if (node.isFolder) {
                var cnt = node.numChildren;
                for (var i = 0; i < cnt; i++) {
                    var node2 = node.getChildAt(i);
                    this.removeNode(node2);
                }
            }
        }
        __cellMouseDown(evt) {
            var node = fgui.GObject.cast(evt.currentTarget)._treeNode;
            this._expandedStatusInEvt = node.expanded;
        }
        __expandedStateChanged(cc) {
            var node = cc.parent._treeNode;
            node.expanded = cc.selectedIndex == 1;
        }
        dispatchItemEvent(item, evt) {
            if (this._clickToExpand != 0) {
                var node = item._treeNode;
                if (node && node.isFolder && this._expandedStatusInEvt == node.expanded) {
                    if (this._clickToExpand == 2) {
                        //if (evt.clickCount == 2)
                        // node.expanded = !node.expanded;
                    }
                    else
                        node.expanded = !node.expanded;
                }
            }
            super.dispatchItemEvent(item, evt);
        }
        setup_beforeAdd(buffer, beginPos) {
            super.setup_beforeAdd(buffer, beginPos);
            buffer.seek(beginPos, 9);
            this._indent = buffer.getInt32();
            this._clickToExpand = buffer.getUint8();
        }
        readItems(buffer) {
            var cnt;
            var i;
            var nextPos;
            var str;
            var isFolder;
            var lastNode;
            var level;
            var prevLevel = 0;
            cnt = buffer.getInt16();
            for (i = 0; i < cnt; i++) {
                nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                str = buffer.readS();
                if (str == null) {
                    str = this.defaultItem;
                    if (!str) {
                        buffer.pos = nextPos;
                        continue;
                    }
                }
                isFolder = buffer.readBool();
                level = buffer.getUint8();
                var node = new fgui.GTreeNode(isFolder, str);
                node.expanded = true;
                if (i == 0)
                    this._rootNode.addChild(node);
                else {
                    if (level > prevLevel)
                        lastNode.addChild(node);
                    else if (level < prevLevel) {
                        for (var j = level; j <= prevLevel; j++)
                            lastNode = lastNode.parent;
                        lastNode.addChild(node);
                    }
                    else
                        lastNode.parent.addChild(node);
                }
                lastNode = node;
                prevLevel = level;
                this.setupItem(buffer, node.cell);
                buffer.pos = nextPos;
            }
        }
    }
    fgui.GTree = GTree;
    var s_list = new Array();
})(fgui);

(function (fgui) {
    class GTreeNode {
        constructor(hasChild, resURL) {
            this._level = 0;
            this._resURL = resURL;
            if (hasChild)
                this._children = new Array();
        }
        set expanded(value) {
            if (this._children == null)
                return;
            if (this._expanded != value) {
                this._expanded = value;
                if (this._tree) {
                    if (this._expanded)
                        this._tree._afterExpanded(this);
                    else
                        this._tree._afterCollapsed(this);
                }
            }
        }
        get expanded() {
            return this._expanded;
        }
        get isFolder() {
            return this._children != null;
        }
        get parent() {
            return this._parent;
        }
        get text() {
            if (this._cell)
                return this._cell.text;
            else
                return null;
        }
        set text(value) {
            if (this._cell)
                this._cell.text = value;
        }
        get icon() {
            if (this._cell)
                return this._cell.icon;
            else
                return null;
        }
        set icon(value) {
            if (this._cell)
                this._cell.icon = value;
        }
        get cell() {
            return this._cell;
        }
        get level() {
            return this._level;
        }
        _setLevel(value) {
            this._level = value;
        }
        addChild(child) {
            this.addChildAt(child, this._children.length);
            return child;
        }
        addChildAt(child, index) {
            if (!child)
                throw new Error("child is null");
            var numChildren = this._children.length;
            if (index >= 0 && index <= numChildren) {
                if (child._parent == this) {
                    this.setChildIndex(child, index);
                }
                else {
                    if (child._parent)
                        child._parent.removeChild(child);
                    var cnt = this._children.length;
                    if (index == cnt)
                        this._children.push(child);
                    else
                        this._children.splice(index, 0, child);
                    child._parent = this;
                    child._level = this._level + 1;
                    child._setTree(this._tree);
                    if (this._tree && this == this._tree.rootNode || this._cell && this._cell.parent && this._expanded)
                        this._tree._afterInserted(child);
                }
                return child;
            }
            else {
                throw new RangeError("Invalid child index");
            }
        }
        removeChild(child) {
            var childIndex = this._children.indexOf(child);
            if (childIndex != -1) {
                this.removeChildAt(childIndex);
            }
            return child;
        }
        removeChildAt(index) {
            if (index >= 0 && index < this.numChildren) {
                var child = this._children[index];
                this._children.splice(index, 1);
                child._parent = null;
                if (this._tree) {
                    child._setTree(null);
                    this._tree._afterRemoved(child);
                }
                return child;
            }
            else {
                throw "Invalid child index";
            }
        }
        removeChildren(beginIndex, endIndex) {
            beginIndex = beginIndex || 0;
            if (endIndex == null)
                endIndex = -1;
            if (endIndex < 0 || endIndex >= this.numChildren)
                endIndex = this.numChildren - 1;
            for (var i = beginIndex; i <= endIndex; ++i)
                this.removeChildAt(beginIndex);
        }
        getChildAt(index) {
            if (index >= 0 && index < this.numChildren)
                return this._children[index];
            else
                throw "Invalid child index";
        }
        getChildIndex(child) {
            return this._children.indexOf(child);
        }
        getPrevSibling() {
            if (this._parent == null)
                return null;
            var i = this._parent._children.indexOf(this);
            if (i <= 0)
                return null;
            return this._parent._children[i - 1];
        }
        getNextSibling() {
            if (this._parent == null)
                return null;
            var i = this._parent._children.indexOf(this);
            if (i < 0 || i >= this._parent._children.length - 1)
                return null;
            return this._parent._children[i + 1];
        }
        setChildIndex(child, index) {
            var oldIndex = this._children.indexOf(child);
            if (oldIndex == -1)
                throw "Not a child of this container";
            var cnt = this._children.length;
            if (index < 0)
                index = 0;
            else if (index > cnt)
                index = cnt;
            if (oldIndex == index)
                return;
            this._children.splice(oldIndex, 1);
            this._children.splice(index, 0, child);
            if (this._tree && this == this._tree.rootNode || this._cell && this._cell.parent && this._expanded)
                this._tree._afterMoved(child);
        }
        swapChildren(child1, child2) {
            var index1 = this._children.indexOf(child1);
            var index2 = this._children.indexOf(child2);
            if (index1 == -1 || index2 == -1)
                throw "Not a child of this container";
            this.swapChildrenAt(index1, index2);
        }
        swapChildrenAt(index1, index2) {
            var child1 = this._children[index1];
            var child2 = this._children[index2];
            this.setChildIndex(child1, index2);
            this.setChildIndex(child2, index1);
        }
        get numChildren() {
            return this._children.length;
        }
        expandToRoot() {
            var p = this;
            while (p) {
                p.expanded = true;
                p = p.parent;
            }
        }
        get tree() {
            return this._tree;
        }
        _setTree(value) {
            this._tree = value;
            if (this._tree && this._tree.treeNodeWillExpand && this._expanded) {
                if (typeof (this._tree.treeNodeWillExpand) == "function")
                    this._tree.treeNodeWillExpand(this, true);
                else
                    this._tree.treeNodeWillExpand.runWith([this, true]);
            }
            if (this._children) {
                var cnt = this._children.length;
                for (var i = 0; i < cnt; i++) {
                    var node = this._children[i];
                    node._level = this._level + 1;
                    node._setTree(value);
                }
            }
        }
    }
    fgui.GTreeNode = GTreeNode;
})(fgui);

(function (fgui) {
    class PackageItem {
        constructor() {
            this.width = 0;
            this.height = 0;
        }
        load() {
            return this.owner.getItemAsset(this);
        }
        getBranch() {
            if (this.branches && this.owner._branchIndex != -1) {
                var itemId = this.branches[this.owner._branchIndex];
                if (itemId)
                    return this.owner.getItemById(itemId);
            }
            return this;
        }
        getHighResolution() {
            if (this.highResolution && fgui.GRoot.contentScaleLevel > 0) {
                var itemId = this.highResolution[fgui.GRoot.contentScaleLevel - 1];
                if (itemId)
                    return this.owner.getItemById(itemId);
            }
            return this;
        }
        toString() {
            return this.name;
        }
    }
    fgui.PackageItem = PackageItem;
})(fgui);

(function (fgui) {
    class PopupMenu {
        constructor(resourceURL) {
            if (!resourceURL) {
                resourceURL = fgui.UIConfig.popupMenu;
                if (!resourceURL)
                    throw "UIConfig.popupMenu not defined";
            }
            this._contentPane = fgui.UIPackage.createObjectFromURL(resourceURL).asCom;
            this._contentPane.on(Laya.Event.DISPLAY, this, this.__addedToStage);
            this._list = (this._contentPane.getChild("list"));
            this._list.removeChildrenToPool();
            this._list.addRelation(this._contentPane, fgui.RelationType.Width);
            this._list.removeRelation(this._contentPane, fgui.RelationType.Height);
            this._contentPane.addRelation(this._list, fgui.RelationType.Height);
            this._list.on(fgui.Events.CLICK_ITEM, this, this.__clickItem);
        }
        dispose() {
            this._contentPane.dispose();
        }
        addItem(caption, handler) {
            var item = this._list.addItemFromPool().asButton;
            item.title = caption;
            item.data = handler;
            item.grayed = false;
            var c = item.getController("checked");
            if (c)
                c.selectedIndex = 0;
            return item;
        }
        addItemAt(caption, index, handler) {
            var item = this._list.getFromPool().asButton;
            this._list.addChildAt(item, index);
            item.title = caption;
            item.data = handler;
            item.grayed = false;
            var c = item.getController("checked");
            if (c)
                c.selectedIndex = 0;
            return item;
        }
        addSeperator() {
            if (fgui.UIConfig.popupMenu_seperator == null)
                throw "UIConfig.popupMenu_seperator not defined";
            this.list.addItemFromPool(fgui.UIConfig.popupMenu_seperator);
        }
        getItemName(index) {
            var item = this._list.getChildAt(index);
            return item.name;
        }
        setItemText(name, caption) {
            var item = this._list.getChild(name).asButton;
            item.title = caption;
        }
        setItemVisible(name, visible) {
            var item = this._list.getChild(name).asButton;
            if (item.visible != visible) {
                item.visible = visible;
                this._list.setBoundsChangedFlag();
            }
        }
        setItemGrayed(name, grayed) {
            var item = this._list.getChild(name).asButton;
            item.grayed = grayed;
        }
        setItemCheckable(name, checkable) {
            var item = this._list.getChild(name).asButton;
            var c = item.getController("checked");
            if (c) {
                if (checkable) {
                    if (c.selectedIndex == 0)
                        c.selectedIndex = 1;
                }
                else
                    c.selectedIndex = 0;
            }
        }
        setItemChecked(name, checked) {
            var item = this._list.getChild(name).asButton;
            var c = item.getController("checked");
            if (c)
                c.selectedIndex = checked ? 2 : 1;
        }
        isItemChecked(name) {
            var item = this._list.getChild(name).asButton;
            var c = item.getController("checked");
            if (c)
                return c.selectedIndex == 2;
            else
                return false;
        }
        removeItem(name) {
            var item = this._list.getChild(name);
            if (item) {
                var index = this._list.getChildIndex(item);
                this._list.removeChildToPoolAt(index);
                return true;
            }
            else
                return false;
        }
        clearItems() {
            this._list.removeChildrenToPool();
        }
        get itemCount() {
            return this._list.numChildren;
        }
        get contentPane() {
            return this._contentPane;
        }
        get list() {
            return this._list;
        }
        show(target = null, dir) {
            var r = target != null ? target.root : fgui.GRoot.inst;
            r.showPopup(this.contentPane, (target instanceof fgui.GRoot) ? null : target, dir);
        }
        __clickItem(itemObject) {
            Laya.timer.once(100, this, this.__clickItem2, [itemObject]);
        }
        __clickItem2(itemObject) {
            if (!(itemObject instanceof fgui.GButton))
                return;
            if (itemObject.grayed) {
                this._list.selectedIndex = -1;
                return;
            }
            var c = itemObject.asCom.getController("checked");
            if (c && c.selectedIndex != 0) {
                if (c.selectedIndex == 1)
                    c.selectedIndex = 2;
                else
                    c.selectedIndex = 1;
            }
            var r = (this._contentPane.parent);
            r.hidePopup(this.contentPane);
            if (typeof itemObject.data === 'function')
                itemObject.data();
            else if (itemObject.data instanceof Laya.Handler)
                itemObject.data.run();
        }
        __addedToStage() {
            this._list.selectedIndex = -1;
            this._list.resizeToFit(100000, 10);
        }
    }
    fgui.PopupMenu = PopupMenu;
})(fgui);

(function (fgui) {
    class RelationItem {
        constructor(owner) {
            this._owner = owner;
            this._defs = new Array();
        }
        get owner() {
            return this._owner;
        }
        set target(value) {
            if (this._target != value) {
                if (this._target)
                    this.releaseRefTarget();
                this._target = value;
                if (this._target)
                    this.addRefTarget();
            }
        }
        get target() {
            return this._target;
        }
        add(relationType, usePercent) {
            if (relationType == fgui.RelationType.Size) {
                this.add(fgui.RelationType.Width, usePercent);
                this.add(fgui.RelationType.Height, usePercent);
                return;
            }
            var cnt = this._defs.length;
            for (var i = 0; i < cnt; i++) {
                if (this._defs[i].type == relationType)
                    return;
            }
            this.internalAdd(relationType, usePercent);
        }
        internalAdd(relationType, usePercent) {
            if (relationType == fgui.RelationType.Size) {
                this.internalAdd(fgui.RelationType.Width, usePercent);
                this.internalAdd(fgui.RelationType.Height, usePercent);
                return;
            }
            var info = new RelationDef();
            info.percent = usePercent;
            info.type = relationType;
            info.axis = (relationType <= fgui.RelationType.Right_Right || relationType == fgui.RelationType.Width || relationType >= fgui.RelationType.LeftExt_Left && relationType <= fgui.RelationType.RightExt_Right) ? 0 : 1;
            this._defs.push(info);
        }
        remove(relationType) {
            if (relationType == fgui.RelationType.Size) {
                this.remove(fgui.RelationType.Width);
                this.remove(fgui.RelationType.Height);
                return;
            }
            var dc = this._defs.length;
            for (var k = 0; k < dc; k++) {
                if (this._defs[k].type == relationType) {
                    this._defs.splice(k, 1);
                    break;
                }
            }
        }
        copyFrom(source) {
            this._target = source.target;
            this._defs.length = 0;
            var cnt = source._defs.length;
            for (var i = 0; i < cnt; i++) {
                var info = source._defs[i];
                var info2 = new RelationDef();
                info2.copyFrom(info);
                this._defs.push(info2);
            }
        }
        dispose() {
            if (this._target) {
                this.releaseRefTarget();
                this._target = null;
            }
        }
        get isEmpty() {
            return this._defs.length == 0;
        }
        applyOnSelfResized(dWidth, dHeight, applyPivot) {
            var cnt = this._defs.length;
            if (cnt == 0)
                return;
            var ox = this._owner.x;
            var oy = this._owner.y;
            for (var i = 0; i < cnt; i++) {
                var info = this._defs[i];
                switch (info.type) {
                    case fgui.RelationType.Center_Center:
                        this._owner.x -= (0.5 - (applyPivot ? this._owner.pivotX : 0)) * dWidth;
                        break;
                    case fgui.RelationType.Right_Center:
                    case fgui.RelationType.Right_Left:
                    case fgui.RelationType.Right_Right:
                        this._owner.x -= (1 - (applyPivot ? this._owner.pivotX : 0)) * dWidth;
                        break;
                    case fgui.RelationType.Middle_Middle:
                        this._owner.y -= (0.5 - (applyPivot ? this._owner.pivotY : 0)) * dHeight;
                        break;
                    case fgui.RelationType.Bottom_Middle:
                    case fgui.RelationType.Bottom_Top:
                    case fgui.RelationType.Bottom_Bottom:
                        this._owner.y -= (1 - (applyPivot ? this._owner.pivotY : 0)) * dHeight;
                        break;
                }
            }
            if (ox != this._owner.x || oy != this._owner.y) {
                ox = this._owner.x - ox;
                oy = this._owner.y - oy;
                this._owner.updateGearFromRelations(1, ox, oy);
                if (this._owner.parent && this._owner.parent._transitions.length > 0) {
                    cnt = this._owner.parent._transitions.length;
                    for (var j = 0; j < cnt; j++) {
                        var trans = this._owner.parent._transitions[j];
                        trans.updateFromRelations(this._owner.id, ox, oy);
                    }
                }
            }
        }
        applyOnXYChanged(info, dx, dy) {
            var tmp;
            switch (info.type) {
                case fgui.RelationType.Left_Left:
                case fgui.RelationType.Left_Center:
                case fgui.RelationType.Left_Right:
                case fgui.RelationType.Center_Center:
                case fgui.RelationType.Right_Left:
                case fgui.RelationType.Right_Center:
                case fgui.RelationType.Right_Right:
                    this._owner.x += dx;
                    break;
                case fgui.RelationType.Top_Top:
                case fgui.RelationType.Top_Middle:
                case fgui.RelationType.Top_Bottom:
                case fgui.RelationType.Middle_Middle:
                case fgui.RelationType.Bottom_Top:
                case fgui.RelationType.Bottom_Middle:
                case fgui.RelationType.Bottom_Bottom:
                    this._owner.y += dy;
                    break;
                case fgui.RelationType.Width:
                case fgui.RelationType.Height:
                    break;
                case fgui.RelationType.LeftExt_Left:
                case fgui.RelationType.LeftExt_Right:
                    if (this._owner != this._target.parent) {
                        tmp = this._owner.xMin;
                        this._owner.width = this._owner._rawWidth - dx;
                        this._owner.xMin = tmp + dx;
                    }
                    else
                        this._owner.width = this._owner._rawWidth - dx;
                    break;
                case fgui.RelationType.RightExt_Left:
                case fgui.RelationType.RightExt_Right:
                    if (this._owner != this._target.parent) {
                        tmp = this._owner.xMin;
                        this._owner.width = this._owner._rawWidth + dx;
                        this._owner.xMin = tmp;
                    }
                    else
                        this._owner.width = this._owner._rawWidth + dx;
                    break;
                case fgui.RelationType.TopExt_Top:
                case fgui.RelationType.TopExt_Bottom:
                    if (this._owner != this._target.parent) {
                        tmp = this._owner.yMin;
                        this._owner.height = this._owner._rawHeight - dy;
                        this._owner.yMin = tmp + dy;
                    }
                    else
                        this._owner.height = this._owner._rawHeight - dy;
                    break;
                case fgui.RelationType.BottomExt_Top:
                case fgui.RelationType.BottomExt_Bottom:
                    if (this._owner != this._target.parent) {
                        tmp = this._owner.yMin;
                        this._owner.height = this._owner._rawHeight + dy;
                        this._owner.yMin = tmp;
                    }
                    else
                        this._owner.height = this._owner._rawHeight + dy;
                    break;
            }
        }
        applyOnSizeChanged(info) {
            var pos = 0, pivot = 0, delta = 0;
            var v, tmp;
            if (info.axis == 0) {
                if (this._target != this._owner.parent) {
                    pos = this._target.x;
                    if (this._target.pivotAsAnchor)
                        pivot = this._target.pivotX;
                }
                if (info.percent) {
                    if (this._targetWidth != 0)
                        delta = this._target._width / this._targetWidth;
                }
                else
                    delta = this._target._width - this._targetWidth;
            }
            else {
                if (this._target != this._owner.parent) {
                    pos = this._target.y;
                    if (this._target.pivotAsAnchor)
                        pivot = this._target.pivotY;
                }
                if (info.percent) {
                    if (this._targetHeight != 0)
                        delta = this._target._height / this._targetHeight;
                }
                else
                    delta = this._target._height - this._targetHeight;
            }
            switch (info.type) {
                case fgui.RelationType.Left_Left:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin - pos) * delta;
                    else if (pivot != 0)
                        this._owner.x += delta * (-pivot);
                    break;
                case fgui.RelationType.Left_Center:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin - pos) * delta;
                    else
                        this._owner.x += delta * (0.5 - pivot);
                    break;
                case fgui.RelationType.Left_Right:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin - pos) * delta;
                    else
                        this._owner.x += delta * (1 - pivot);
                    break;
                case fgui.RelationType.Center_Center:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin + this._owner._rawWidth * 0.5 - pos) * delta - this._owner._rawWidth * 0.5;
                    else
                        this._owner.x += delta * (0.5 - pivot);
                    break;
                case fgui.RelationType.Right_Left:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin + this._owner._rawWidth - pos) * delta - this._owner._rawWidth;
                    else if (pivot != 0)
                        this._owner.x += delta * (-pivot);
                    break;
                case fgui.RelationType.Right_Center:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin + this._owner._rawWidth - pos) * delta - this._owner._rawWidth;
                    else
                        this._owner.x += delta * (0.5 - pivot);
                    break;
                case fgui.RelationType.Right_Right:
                    if (info.percent)
                        this._owner.xMin = pos + (this._owner.xMin + this._owner._rawWidth - pos) * delta - this._owner._rawWidth;
                    else
                        this._owner.x += delta * (1 - pivot);
                    break;
                case fgui.RelationType.Top_Top:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin - pos) * delta;
                    else if (pivot != 0)
                        this._owner.y += delta * (-pivot);
                    break;
                case fgui.RelationType.Top_Middle:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin - pos) * delta;
                    else
                        this._owner.y += delta * (0.5 - pivot);
                    break;
                case fgui.RelationType.Top_Bottom:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin - pos) * delta;
                    else
                        this._owner.y += delta * (1 - pivot);
                    break;
                case fgui.RelationType.Middle_Middle:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin + this._owner._rawHeight * 0.5 - pos) * delta - this._owner._rawHeight * 0.5;
                    else
                        this._owner.y += delta * (0.5 - pivot);
                    break;
                case fgui.RelationType.Bottom_Top:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin + this._owner._rawHeight - pos) * delta - this._owner._rawHeight;
                    else if (pivot != 0)
                        this._owner.y += delta * (-pivot);
                    break;
                case fgui.RelationType.Bottom_Middle:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin + this._owner._rawHeight - pos) * delta - this._owner._rawHeight;
                    else
                        this._owner.y += delta * (0.5 - pivot);
                    break;
                case fgui.RelationType.Bottom_Bottom:
                    if (info.percent)
                        this._owner.yMin = pos + (this._owner.yMin + this._owner._rawHeight - pos) * delta - this._owner._rawHeight;
                    else
                        this._owner.y += delta * (1 - pivot);
                    break;
                case fgui.RelationType.Width:
                    if ( /*this._owner._underConstruct && */this._owner == this._target.parent)
                        v = this._owner.sourceWidth - this._target.initWidth;
                    else
                        v = this._owner._rawWidth - this._targetWidth;
                    if (info.percent)
                        v = v * delta;
                    if (this._target == this._owner.parent) {
                        if (this._owner.pivotAsAnchor) {
                            tmp = this._owner.xMin;
                            this._owner.setSize(this._target._width + v, this._owner._rawHeight, true);
                            this._owner.xMin = tmp;
                        }
                        else
                            this._owner.setSize(this._target._width + v, this._owner._rawHeight, true);
                    }
                    else
                        this._owner.width = this._target._width + v;
                    break;
                case fgui.RelationType.Height:
                    if ( /*this._owner._underConstruct && */this._owner == this._target.parent)
                        v = this._owner.sourceHeight - this._target.initHeight;
                    else
                        v = this._owner._rawHeight - this._targetHeight;
                    if (info.percent)
                        v = v * delta;
                    if (this._target == this._owner.parent) {
                        if (this._owner.pivotAsAnchor) {
                            tmp = this._owner.yMin;
                            this._owner.setSize(this._owner._rawWidth, this._target._height + v, true);
                            this._owner.yMin = tmp;
                        }
                        else
                            this._owner.setSize(this._owner._rawWidth, this._target._height + v, true);
                    }
                    else
                        this._owner.height = this._target._height + v;
                    break;
                case fgui.RelationType.LeftExt_Left:
                    tmp = this._owner.xMin;
                    if (info.percent)
                        v = pos + (tmp - pos) * delta - tmp;
                    else
                        v = delta * (-pivot);
                    this._owner.width = this._owner._rawWidth - v;
                    this._owner.xMin = tmp + v;
                    break;
                case fgui.RelationType.LeftExt_Right:
                    tmp = this._owner.xMin;
                    if (info.percent)
                        v = pos + (tmp - pos) * delta - tmp;
                    else
                        v = delta * (1 - pivot);
                    this._owner.width = this._owner._rawWidth - v;
                    this._owner.xMin = tmp + v;
                    break;
                case fgui.RelationType.RightExt_Left:
                    tmp = this._owner.xMin;
                    if (info.percent)
                        v = pos + (tmp + this._owner._rawWidth - pos) * delta - (tmp + this._owner._rawWidth);
                    else
                        v = delta * (-pivot);
                    this._owner.width = this._owner._rawWidth + v;
                    this._owner.xMin = tmp;
                    break;
                case fgui.RelationType.RightExt_Right:
                    tmp = this._owner.xMin;
                    if (info.percent) {
                        if (this._owner == this._target.parent) {
                            //if (this._owner._underConstruct)
                            this._owner.width = pos + this._target._width - this._target._width * pivot +
                                (this._owner.sourceWidth - this._targetInitX - this._target.initWidth + this._target.initWidth * pivot) * delta;
                            //else
                            //    this._owner.width = pos + (this._owner._rawWidth - pos) * delta;
                        }
                        else {
                            v = pos + (tmp + this._owner._rawWidth - pos) * delta - (tmp + this._owner._rawWidth);
                            this._owner.width = this._owner._rawWidth + v;
                            this._owner.xMin = tmp;
                        }
                    }
                    else {
                        if (this._owner == this._target.parent) {
                            //if (this._owner._underConstruct)
                            this._owner.width = this._owner.sourceWidth + pos - this._targetInitX + (this._target._width - this._target.initWidth) * (1 - pivot);
                            //else
                            //   this._owner.width = this._owner._rawWidth + delta * (1 - pivot);
                        }
                        else {
                            v = delta * (1 - pivot);
                            this._owner.width = this._owner._rawWidth + v;
                            this._owner.xMin = tmp;
                        }
                    }
                    break;
                case fgui.RelationType.TopExt_Top:
                    tmp = this._owner.yMin;
                    if (info.percent)
                        v = pos + (tmp - pos) * delta - tmp;
                    else
                        v = delta * (-pivot);
                    this._owner.height = this._owner._rawHeight - v;
                    this._owner.yMin = tmp + v;
                    break;
                case fgui.RelationType.TopExt_Bottom:
                    tmp = this._owner.yMin;
                    if (info.percent)
                        v = pos + (tmp - pos) * delta - tmp;
                    else
                        v = delta * (1 - pivot);
                    this._owner.height = this._owner._rawHeight - v;
                    this._owner.yMin = tmp + v;
                    break;
                case fgui.RelationType.BottomExt_Top:
                    tmp = this._owner.yMin;
                    if (info.percent)
                        v = pos + (tmp + this._owner._rawHeight - pos) * delta - (tmp + this._owner._rawHeight);
                    else
                        v = delta * (-pivot);
                    this._owner.height = this._owner._rawHeight + v;
                    this._owner.yMin = tmp;
                    break;
                case fgui.RelationType.BottomExt_Bottom:
                    tmp = this._owner.yMin;
                    if (info.percent) {
                        if (this._owner == this._target.parent) {
                            // if (this._owner._underConstruct)
                            this._owner.height = pos + this._target._height - this._target._height * pivot +
                                (this._owner.sourceHeight - this._targetInitY - this._target.initHeight + this._target.initHeight * pivot) * delta;
                            //else
                            //    this._owner.height = pos + (this._owner._rawHeight - pos) * delta;
                        }
                        else {
                            v = pos + (tmp + this._owner._rawHeight - pos) * delta - (tmp + this._owner._rawHeight);
                            this._owner.height = this._owner._rawHeight + v;
                            this._owner.yMin = tmp;
                        }
                    }
                    else {
                        if (this._owner == this._target.parent) {
                            //if (this._owner._underConstruct)
                            this._owner.height = this._owner.sourceHeight + pos - this._targetInitY + (this._target._height - this._target.initHeight) * (1 - pivot);
                            //else
                            //    this._owner.height = this._owner._rawHeight + delta * (1 - pivot);
                        }
                        else {
                            v = delta * (1 - pivot);
                            this._owner.height = this._owner._rawHeight + v;
                            this._owner.yMin = tmp;
                        }
                    }
                    break;
            }
        }
        addRefTarget() {
            if (this._target != this._owner.parent)
                this._target.on(fgui.Events.XY_CHANGED, this, this.__targetXYChanged);
            this._target.on(fgui.Events.SIZE_CHANGED, this, this.__targetSizeChanged);
            this._target.on(fgui.Events.SIZE_DELAY_CHANGE, this, this.__targetSizeWillChange);
            this._targetX = this._targetInitX = this._target.x;
            this._targetY = this._targetInitY = this._target.y;
            this._targetWidth = this._target._width;
            this._targetHeight = this._target._height;
        }
        releaseRefTarget() {
            if (this._target.displayObject == null)
                return;
            this._target.off(fgui.Events.XY_CHANGED, this, this.__targetXYChanged);
            this._target.off(fgui.Events.SIZE_CHANGED, this, this.__targetSizeChanged);
            this._target.off(fgui.Events.SIZE_DELAY_CHANGE, this, this.__targetSizeWillChange);
        }
        __targetXYChanged() {
            if (this._owner.relations.handling != null || this._owner.group != null && this._owner.group._updating) {
                this._targetX = this._target.x;
                this._targetY = this._target.y;
                return;
            }
            this._owner.relations.handling = this._target;
            var ox = this._owner.x;
            var oy = this._owner.y;
            var dx = this._target.x - this._targetX;
            var dy = this._target.y - this._targetY;
            var cnt = this._defs.length;
            for (var i = 0; i < cnt; i++) {
                this.applyOnXYChanged(this._defs[i], dx, dy);
            }
            this._targetX = this._target.x;
            this._targetY = this._target.y;
            if (ox != this._owner.x || oy != this._owner.y) {
                ox = this._owner.x - ox;
                oy = this._owner.y - oy;
                this._owner.updateGearFromRelations(1, ox, oy);
                if (this._owner.parent && this._owner.parent._transitions.length > 0) {
                    cnt = this._owner.parent._transitions.length;
                    for (var j = 0; j < cnt; j++) {
                        var trans = this._owner.parent._transitions[j];
                        trans.updateFromRelations(this._owner.id, ox, oy);
                    }
                }
            }
            this._owner.relations.handling = null;
        }
        __targetSizeChanged() {
            if (this._owner.relations.sizeDirty)
                this._owner.relations.ensureRelationsSizeCorrect();
            if (this._owner.relations.handling != null) {
                this._targetWidth = this._target._width;
                this._targetHeight = this._target._height;
                return;
            }
            this._owner.relations.handling = this._target;
            var ox = this._owner.x;
            var oy = this._owner.y;
            var ow = this._owner._rawWidth;
            var oh = this._owner._rawHeight;
            var cnt = this._defs.length;
            for (var i = 0; i < cnt; i++) {
                this.applyOnSizeChanged(this._defs[i]);
            }
            this._targetWidth = this._target._width;
            this._targetHeight = this._target._height;
            if (ox != this._owner.x || oy != this._owner.y) {
                ox = this._owner.x - ox;
                oy = this._owner.y - oy;
                this._owner.updateGearFromRelations(1, ox, oy);
                if (this._owner.parent && this._owner.parent._transitions.length > 0) {
                    cnt = this._owner.parent._transitions.length;
                    for (var j = 0; j < cnt; j++) {
                        var trans = this._owner.parent._transitions[j];
                        trans.updateFromRelations(this._owner.id, ox, oy);
                    }
                }
            }
            if (ow != this._owner._rawWidth || oh != this._owner._rawHeight) {
                ow = this._owner._rawWidth - ow;
                oh = this._owner._rawHeight - oh;
                this._owner.updateGearFromRelations(2, ow, oh);
            }
            this._owner.relations.handling = null;
        }
        __targetSizeWillChange() {
            this._owner.relations.sizeDirty = true;
        }
    }
    fgui.RelationItem = RelationItem;
    class RelationDef {
        constructor() {
        }
        copyFrom(source) {
            this.percent = source.percent;
            this.type = source.type;
            this.axis = source.axis;
        }
    }
})(fgui);

(function (fgui) {
    class Relations {
        constructor(owner) {
            this._owner = owner;
            this._items = [];
        }
        add(target, relationType, usePercent) {
            var length = this._items.length;
            for (var i = 0; i < length; i++) {
                var item = this._items[i];
                if (item.target == target) {
                    item.add(relationType, usePercent);
                    return;
                }
            }
            var newItem = new fgui.RelationItem(this._owner);
            newItem.target = target;
            newItem.add(relationType, usePercent);
            this._items.push(newItem);
        }
        remove(target, relationType) {
            relationType = relationType || 0;
            var cnt = this._items.length;
            var i = 0;
            while (i < cnt) {
                var item = this._items[i];
                if (item.target == target) {
                    item.remove(relationType);
                    if (item.isEmpty) {
                        item.dispose();
                        this._items.splice(i, 1);
                        cnt--;
                    }
                    else
                        i++;
                }
                else
                    i++;
            }
        }
        contains(target) {
            var length = this._items.length;
            for (var i = 0; i < length; i++) {
                var item = this._items[i];
                if (item.target == target)
                    return true;
            }
            return false;
        }
        clearFor(target) {
            var cnt = this._items.length;
            var i = 0;
            while (i < cnt) {
                var item = this._items[i];
                if (item.target == target) {
                    item.dispose();
                    this._items.splice(i, 1);
                    cnt--;
                }
                else
                    i++;
            }
        }
        clearAll() {
            var length = this._items.length;
            for (var i = 0; i < length; i++) {
                var item = this._items[i];
                item.dispose();
            }
            this._items.length = 0;
        }
        copyFrom(source) {
            this.clearAll();
            var arr = source._items;
            var length = arr.length;
            for (var i = 0; i < length; i++) {
                var ri = arr[i];
                var item = new fgui.RelationItem(this._owner);
                item.copyFrom(ri);
                this._items.push(item);
            }
        }
        dispose() {
            this.clearAll();
        }
        onOwnerSizeChanged(dWidth, dHeight, applyPivot) {
            if (this._items.length == 0)
                return;
            var length = this._items.length;
            for (var i = 0; i < length; i++) {
                var item = this._items[i];
                item.applyOnSelfResized(dWidth, dHeight, applyPivot);
            }
        }
        ensureRelationsSizeCorrect() {
            if (this._items.length == 0)
                return;
            this.sizeDirty = false;
            var length = this._items.length;
            for (var i = 0; i < length; i++) {
                var item = this._items[i];
                item.target.ensureSizeCorrect();
            }
        }
        get empty() {
            return this._items.length == 0;
        }
        setup(buffer, parentToChild) {
            var cnt = buffer.readByte();
            var target;
            for (var i = 0; i < cnt; i++) {
                var targetIndex = buffer.getInt16();
                if (targetIndex == -1)
                    target = this._owner.parent;
                else if (parentToChild)
                    target = (this._owner).getChildAt(targetIndex);
                else
                    target = this._owner.parent.getChildAt(targetIndex);
                var newItem = new fgui.RelationItem(this._owner);
                newItem.target = target;
                this._items.push(newItem);
                var cnt2 = buffer.readByte();
                for (var j = 0; j < cnt2; j++) {
                    var rt = buffer.readByte();
                    var usePercent = buffer.readBool();
                    newItem.internalAdd(rt, usePercent);
                }
            }
        }
    }
    fgui.Relations = Relations;
})(fgui);

(function (fgui) {
    class ScrollPane {
        constructor(owner) {
            this._owner = owner;
            this._maskContainer = new Laya.Sprite();
            this._owner.displayObject.addChild(this._maskContainer);
            this._container = this._owner._container;
            this._container.pos(0, 0);
            this._maskContainer.addChild(this._container);
            this._mouseWheelEnabled = true;
            this._xPos = 0;
            this._yPos = 0;
            this._aniFlag = 0;
            this._tweening = 0;
            this._loop = 0;
            this._footerLockedSize = 0;
            this._headerLockedSize = 0;
            this._scrollBarMargin = new fgui.Margin();
            this._viewSize = new Laya.Point();
            this._contentSize = new Laya.Point();
            this._pageSize = new Laya.Point(1, 1);
            this._overlapSize = new Laya.Point();
            this._tweenTime = new Laya.Point();
            this._tweenStart = new Laya.Point();
            this._tweenDuration = new Laya.Point();
            this._tweenChange = new Laya.Point();
            this._velocity = new Laya.Point();
            this._containerPos = new Laya.Point();
            this._beginTouchPos = new Laya.Point();
            this._lastTouchPos = new Laya.Point();
            this._lastTouchGlobalPos = new Laya.Point();
            this._scrollStep = fgui.UIConfig.defaultScrollStep;
            this._mouseWheelStep = this._scrollStep * 2;
            this._decelerationRate = fgui.UIConfig.defaultScrollDecelerationRate;
            this._owner.on(Laya.Event.MOUSE_DOWN, this, this.__mouseDown);
            this._owner.on(Laya.Event.MOUSE_WHEEL, this, this.__mouseWheel);
        }
        setup(buffer) {
            this._scrollType = buffer.readByte();
            var scrollBarDisplay = buffer.readByte();
            var flags = buffer.getInt32();
            if (buffer.readBool()) {
                this._scrollBarMargin.top = buffer.getInt32();
                this._scrollBarMargin.bottom = buffer.getInt32();
                this._scrollBarMargin.left = buffer.getInt32();
                this._scrollBarMargin.right = buffer.getInt32();
            }
            var vtScrollBarRes = buffer.readS();
            var hzScrollBarRes = buffer.readS();
            var headerRes = buffer.readS();
            var footerRes = buffer.readS();
            if ((flags & 1) != 0)
                this._displayOnLeft = true;
            if ((flags & 2) != 0)
                this._snapToItem = true;
            if ((flags & 4) != 0)
                this._displayInDemand = true;
            if ((flags & 8) != 0)
                this._pageMode = true;
            if (flags & 16)
                this._touchEffect = true;
            else if (flags & 32)
                this._touchEffect = false;
            else
                this._touchEffect = fgui.UIConfig.defaultScrollTouchEffect;
            if (flags & 64)
                this._bouncebackEffect = true;
            else if (flags & 128)
                this._bouncebackEffect = false;
            else
                this._bouncebackEffect = fgui.UIConfig.defaultScrollBounceEffect;
            if ((flags & 256) != 0)
                this._inertiaDisabled = true;
            if ((flags & 512) == 0)
                this._maskContainer.scrollRect = new Laya.Rectangle();
            if ((flags & 1024) != 0)
                this._floating = true;
            if ((flags & 2048) != 0)
                this._dontClipMargin = true;
            if (scrollBarDisplay == fgui.ScrollBarDisplayType.Default)
                scrollBarDisplay = fgui.UIConfig.defaultScrollBarDisplay;
            if (scrollBarDisplay != fgui.ScrollBarDisplayType.Hidden) {
                if (this._scrollType == fgui.ScrollType.Both || this._scrollType == fgui.ScrollType.Vertical) {
                    var res = vtScrollBarRes ? vtScrollBarRes : fgui.UIConfig.verticalScrollBar;
                    if (res) {
                        this._vtScrollBar = (fgui.UIPackage.createObjectFromURL(res));
                        if (!this._vtScrollBar)
                            throw "cannot create scrollbar from " + res;
                        this._vtScrollBar.setScrollPane(this, true);
                        this._owner.displayObject.addChild(this._vtScrollBar.displayObject);
                    }
                }
                if (this._scrollType == fgui.ScrollType.Both || this._scrollType == fgui.ScrollType.Horizontal) {
                    res = hzScrollBarRes ? hzScrollBarRes : fgui.UIConfig.horizontalScrollBar;
                    if (res) {
                        this._hzScrollBar = (fgui.UIPackage.createObjectFromURL(res));
                        if (!this._hzScrollBar)
                            throw "cannot create scrollbar from " + res;
                        this._hzScrollBar.setScrollPane(this, false);
                        this._owner.displayObject.addChild(this._hzScrollBar.displayObject);
                    }
                }
                if (scrollBarDisplay == fgui.ScrollBarDisplayType.Auto)
                    this._scrollBarDisplayAuto = true;
                if (this._scrollBarDisplayAuto) {
                    if (this._vtScrollBar)
                        this._vtScrollBar.displayObject.visible = false;
                    if (this._hzScrollBar)
                        this._hzScrollBar.displayObject.visible = false;
                }
            }
            else
                this._mouseWheelEnabled = false;
            if (headerRes) {
                this._header = fgui.UIPackage.createObjectFromURL(headerRes);
                if (!this._header)
                    throw new Error("FairyGUI: cannot create scrollPane this.header from " + headerRes);
            }
            if (footerRes) {
                this._footer = fgui.UIPackage.createObjectFromURL(footerRes);
                if (!this._footer)
                    throw new Error("FairyGUI: cannot create scrollPane this.footer from " + footerRes);
            }
            if (this._header || this._footer)
                this._refreshBarAxis = (this._scrollType == fgui.ScrollType.Both || this._scrollType == fgui.ScrollType.Vertical) ? "y" : "x";
            this.setSize(this.owner.width, this.owner.height);
        }
        dispose() {
            if (ScrollPane.draggingPane == this) {
                ScrollPane.draggingPane = null;
            }
            if (this._tweening != 0)
                Laya.timer.clear(this, this.tweenUpdate);
            this._pageController = null;
            if (this._hzScrollBar)
                this._hzScrollBar.dispose();
            if (this._vtScrollBar)
                this._vtScrollBar.dispose();
            if (this._header)
                this._header.dispose();
            if (this._footer)
                this._footer.dispose();
        }
        get owner() {
            return this._owner;
        }
        get hzScrollBar() {
            return this._hzScrollBar;
        }
        get vtScrollBar() {
            return this._vtScrollBar;
        }
        get header() {
            return this._header;
        }
        get footer() {
            return this._footer;
        }
        get bouncebackEffect() {
            return this._bouncebackEffect;
        }
        set bouncebackEffect(sc) {
            this._bouncebackEffect = sc;
        }
        get touchEffect() {
            return this._touchEffect;
        }
        set touchEffect(sc) {
            this._touchEffect = sc;
        }
        set scrollStep(val) {
            this._scrollStep = val;
            if (this._scrollStep == 0)
                this._scrollStep = fgui.UIConfig.defaultScrollStep;
            this._mouseWheelStep = this._scrollStep * 2;
        }
        get scrollStep() {
            return this._scrollStep;
        }
        get snapToItem() {
            return this._snapToItem;
        }
        set snapToItem(value) {
            this._snapToItem = value;
        }
        get mouseWheelEnabled() {
            return this._mouseWheelEnabled;
        }
        set mouseWheelEnabled(value) {
            this._mouseWheelEnabled = value;
        }
        get decelerationRate() {
            return this._decelerationRate;
        }
        set decelerationRate(value) {
            this._decelerationRate = value;
        }
        get isDragged() {
            return this._dragged;
        }
        get percX() {
            return this._overlapSize.x == 0 ? 0 : this._xPos / this._overlapSize.x;
        }
        set percX(value) {
            this.setPercX(value, false);
        }
        setPercX(value, ani) {
            this._owner.ensureBoundsCorrect();
            this.setPosX(this._overlapSize.x * fgui.ToolSet.clamp01(value), ani);
        }
        get percY() {
            return this._overlapSize.y == 0 ? 0 : this._yPos / this._overlapSize.y;
        }
        set percY(value) {
            this.setPercY(value, false);
        }
        setPercY(value, ani) {
            this._owner.ensureBoundsCorrect();
            this.setPosY(this._overlapSize.y * fgui.ToolSet.clamp01(value), ani);
        }
        get posX() {
            return this._xPos;
        }
        set posX(value) {
            this.setPosX(value, false);
        }
        setPosX(value, ani) {
            this._owner.ensureBoundsCorrect();
            if (this._loop == 1)
                value = this.loopCheckingNewPos(value, "x");
            value = fgui.ToolSet.clamp(value, 0, this._overlapSize.x);
            if (value != this._xPos) {
                this._xPos = value;
                this.posChanged(ani);
            }
        }
        get posY() {
            return this._yPos;
        }
        set posY(value) {
            this.setPosY(value, false);
        }
        setPosY(value, ani) {
            this._owner.ensureBoundsCorrect();
            if (this._loop == 1)
                value = this.loopCheckingNewPos(value, "y");
            value = fgui.ToolSet.clamp(value, 0, this._overlapSize.y);
            if (value != this._yPos) {
                this._yPos = value;
                this.posChanged(ani);
            }
        }
        get contentWidth() {
            return this._contentSize.x;
        }
        get contentHeight() {
            return this._contentSize.y;
        }
        get viewWidth() {
            return this._viewSize.x;
        }
        set viewWidth(value) {
            value = value + this._owner.margin.left + this._owner.margin.right;
            if (this._vtScrollBar && !this._floating)
                value += this._vtScrollBar.width;
            this._owner.width = value;
        }
        get viewHeight() {
            return this._viewSize.y;
        }
        set viewHeight(value) {
            value = value + this._owner.margin.top + this._owner.margin.bottom;
            if (this._hzScrollBar && !this._floating)
                value += this._hzScrollBar.height;
            this._owner.height = value;
        }
        get currentPageX() {
            if (!this._pageMode)
                return 0;
            var page = Math.floor(this._xPos / this._pageSize.x);
            if (this._xPos - page * this._pageSize.x > this._pageSize.x * 0.5)
                page++;
            return page;
        }
        set currentPageX(value) {
            this.setCurrentPageX(value, false);
        }
        get currentPageY() {
            if (!this._pageMode)
                return 0;
            var page = Math.floor(this._yPos / this._pageSize.y);
            if (this._yPos - page * this._pageSize.y > this._pageSize.y * 0.5)
                page++;
            return page;
        }
        set currentPageY(value) {
            this.setCurrentPageY(value, false);
        }
        setCurrentPageX(value, ani) {
            if (!this._pageMode)
                return;
            this._owner.ensureBoundsCorrect();
            if (this._overlapSize.x > 0)
                this.setPosX(value * this._pageSize.x, ani);
        }
        setCurrentPageY(value, ani) {
            if (!this._pageMode)
                return;
            this._owner.ensureBoundsCorrect();
            if (this._overlapSize.y > 0)
                this.setPosY(value * this._pageSize.y, ani);
        }
        get isBottomMost() {
            return this._yPos == this._overlapSize.y || this._overlapSize.y == 0;
        }
        get isRightMost() {
            return this._xPos == this._overlapSize.x || this._overlapSize.x == 0;
        }
        get pageController() {
            return this._pageController;
        }
        set pageController(value) {
            this._pageController = value;
        }
        get scrollingPosX() {
            return fgui.ToolSet.clamp(-this._container.x, 0, this._overlapSize.x);
        }
        get scrollingPosY() {
            return fgui.ToolSet.clamp(-this._container.y, 0, this._overlapSize.y);
        }
        scrollTop(ani) {
            this.setPercY(0, ani);
        }
        scrollBottom(ani) {
            this.setPercY(1, ani);
        }
        scrollUp(ratio, ani) {
            ratio = ratio || 1;
            if (this._pageMode)
                this.setPosY(this._yPos - this._pageSize.y * ratio, ani);
            else
                this.setPosY(this._yPos - this._scrollStep * ratio, ani);
            ;
        }
        scrollDown(ratio, ani) {
            ratio = ratio || 1;
            if (this._pageMode)
                this.setPosY(this._yPos + this._pageSize.y * ratio, ani);
            else
                this.setPosY(this._yPos + this._scrollStep * ratio, ani);
        }
        scrollLeft(ratio, ani) {
            ratio = ratio || 1;
            if (this._pageMode)
                this.setPosX(this._xPos - this._pageSize.x * ratio, ani);
            else
                this.setPosX(this._xPos - this._scrollStep * ratio, ani);
        }
        scrollRight(ratio, ani) {
            ratio = ratio || 1;
            if (this._pageMode)
                this.setPosX(this._xPos + this._pageSize.x * ratio, ani);
            else
                this.setPosX(this._xPos + this._scrollStep * ratio, ani);
        }
        scrollToView(target, ani, setFirst) {
            this._owner.ensureBoundsCorrect();
            if (this._needRefresh)
                this.refresh();
            var rect;
            if (target instanceof fgui.GObject) {
                if (target.parent != this._owner) {
                    target.parent.localToGlobalRect(target.x, target.y, target.width, target.height, s_rect);
                    rect = this._owner.globalToLocalRect(s_rect.x, s_rect.y, s_rect.width, s_rect.height, s_rect);
                }
                else {
                    rect = s_rect;
                    rect.setTo(target.x, target.y, target.width, target.height);
                }
            }
            else
                rect = target;
            if (this._overlapSize.y > 0) {
                var bottom = this._yPos + this._viewSize.y;
                if (setFirst || rect.y <= this._yPos || rect.height >= this._viewSize.y) {
                    if (this._pageMode)
                        this.setPosY(Math.floor(rect.y / this._pageSize.y) * this._pageSize.y, ani);
                    else
                        this.setPosY(rect.y, ani);
                }
                else if (rect.y + rect.height > bottom) {
                    if (this._pageMode)
                        this.setPosY(Math.floor(rect.y / this._pageSize.y) * this._pageSize.y, ani);
                    else if (rect.height <= this._viewSize.y / 2)
                        this.setPosY(rect.y + rect.height * 2 - this._viewSize.y, ani);
                    else
                        this.setPosY(rect.y + rect.height - this._viewSize.y, ani);
                }
            }
            if (this._overlapSize.x > 0) {
                var right = this._xPos + this._viewSize.x;
                if (setFirst || rect.x <= this._xPos || rect.width >= this._viewSize.x) {
                    if (this._pageMode)
                        this.setPosX(Math.floor(rect.x / this._pageSize.x) * this._pageSize.x, ani);
                    else
                        this.setPosX(rect.x, ani);
                }
                else if (rect.x + rect.width > right) {
                    if (this._pageMode)
                        this.setPosX(Math.floor(rect.x / this._pageSize.x) * this._pageSize.x, ani);
                    else if (rect.width <= this._viewSize.x / 2)
                        this.setPosX(rect.x + rect.width * 2 - this._viewSize.x, ani);
                    else
                        this.setPosX(rect.x + rect.width - this._viewSize.x, ani);
                }
            }
            if (!ani && this._needRefresh)
                this.refresh();
        }
        isChildInView(obj) {
            if (this._overlapSize.y > 0) {
                var dist = obj.y + this._container.y;
                if (dist < -obj.height || dist > this._viewSize.y)
                    return false;
            }
            if (this._overlapSize.x > 0) {
                dist = obj.x + this._container.x;
                if (dist < -obj.width || dist > this._viewSize.x)
                    return false;
            }
            return true;
        }
        cancelDragging() {
            this._owner.displayObject.stage.off(Laya.Event.MOUSE_MOVE, this, this.__mouseMove);
            this._owner.displayObject.stage.off(Laya.Event.MOUSE_UP, this, this.__mouseUp);
            this._owner.displayObject.stage.off(Laya.Event.CLICK, this, this.__click);
            if (ScrollPane.draggingPane == this)
                ScrollPane.draggingPane = null;
            _gestureFlag = 0;
            this._dragged = false;
            this._maskContainer.mouseEnabled = true;
        }
        lockHeader(size) {
            if (this._headerLockedSize == size)
                return;
            this._headerLockedSize = size;
            if (!this._refreshEventDispatching && this._container[this._refreshBarAxis] >= 0) {
                this._tweenStart.setTo(this._container.x, this._container.y);
                this._tweenChange.setTo(0, 0);
                this._tweenChange[this._refreshBarAxis] = this._headerLockedSize - this._tweenStart[this._refreshBarAxis];
                this._tweenDuration.setTo(TWEEN_TIME_DEFAULT, TWEEN_TIME_DEFAULT);
                this.startTween(2);
            }
        }
        lockFooter(size) {
            if (this._footerLockedSize == size)
                return;
            this._footerLockedSize = size;
            if (!this._refreshEventDispatching && this._container[this._refreshBarAxis] <= -this._overlapSize[this._refreshBarAxis]) {
                this._tweenStart.setTo(this._container.x, this._container.y);
                this._tweenChange.setTo(0, 0);
                var max = this._overlapSize[this._refreshBarAxis];
                if (max == 0)
                    max = Math.max(this._contentSize[this._refreshBarAxis] + this._footerLockedSize - this._viewSize[this._refreshBarAxis], 0);
                else
                    max += this._footerLockedSize;
                this._tweenChange[this._refreshBarAxis] = -max - this._tweenStart[this._refreshBarAxis];
                this._tweenDuration.setTo(TWEEN_TIME_DEFAULT, TWEEN_TIME_DEFAULT);
                this.startTween(2);
            }
        }
        onOwnerSizeChanged() {
            this.setSize(this._owner.width, this._owner.height);
            this.posChanged(false);
        }
        handleControllerChanged(c) {
            if (this._pageController == c) {
                if (this._scrollType == fgui.ScrollType.Horizontal)
                    this.setCurrentPageX(c.selectedIndex, true);
                else
                    this.setCurrentPageY(c.selectedIndex, true);
            }
        }
        updatePageController() {
            if (this._pageController != null && !this._pageController.changing) {
                var index;
                if (this._scrollType == fgui.ScrollType.Horizontal)
                    index = this.currentPageX;
                else
                    index = this.currentPageY;
                if (index < this._pageController.pageCount) {
                    var c = this._pageController;
                    this._pageController = null; //防止HandleControllerChanged的调用
                    c.selectedIndex = index;
                    this._pageController = c;
                }
            }
        }
        adjustMaskContainer() {
            var mx = 0, my = 0;
            if (this._dontClipMargin) {
                if (this._displayOnLeft && this._vtScrollBar && !this._floating)
                    mx = this._vtScrollBar.width;
            }
            else {
                if (this._displayOnLeft && this._vtScrollBar && !this._floating)
                    mx = this._owner.margin.left + this._vtScrollBar.width;
                else
                    mx = this._owner.margin.left;
                my = this._owner.margin.top;
            }
            this._maskContainer.pos(mx, my);
            mx = this._owner._alignOffset.x;
            my = this._owner._alignOffset.y;
            if (mx != 0 || my != 0 || this._dontClipMargin) {
                if (!this._alignContainer) {
                    this._alignContainer = new Laya.Sprite();
                    this._maskContainer.addChild(this._alignContainer);
                    this._alignContainer.addChild(this._container);
                }
            }
            if (this._alignContainer) {
                if (this._dontClipMargin) {
                    mx += this._owner.margin.left;
                    my += this._owner.margin.top;
                }
                this._alignContainer.pos(mx, my);
            }
        }
        setSize(aWidth, aHeight) {
            this.adjustMaskContainer();
            if (this._hzScrollBar) {
                this._hzScrollBar.y = aHeight - this._hzScrollBar.height;
                if (this._vtScrollBar) {
                    this._hzScrollBar.width = aWidth - this._vtScrollBar.width - this._scrollBarMargin.left - this._scrollBarMargin.right;
                    if (this._displayOnLeft)
                        this._hzScrollBar.x = this._scrollBarMargin.left + this._vtScrollBar.width;
                    else
                        this._hzScrollBar.x = this._scrollBarMargin.left;
                }
                else {
                    this._hzScrollBar.width = aWidth - this._scrollBarMargin.left - this._scrollBarMargin.right;
                    this._hzScrollBar.x = this._scrollBarMargin.left;
                }
            }
            if (this._vtScrollBar) {
                if (!this._displayOnLeft)
                    this._vtScrollBar.x = aWidth - this._vtScrollBar.width;
                if (this._hzScrollBar)
                    this._vtScrollBar.height = aHeight - this._hzScrollBar.height - this._scrollBarMargin.top - this._scrollBarMargin.bottom;
                else
                    this._vtScrollBar.height = aHeight - this._scrollBarMargin.top - this._scrollBarMargin.bottom;
                this._vtScrollBar.y = this._scrollBarMargin.top;
            }
            this._viewSize.x = aWidth;
            this._viewSize.y = aHeight;
            if (this._hzScrollBar && !this._floating)
                this._viewSize.y -= this._hzScrollBar.height;
            if (this._vtScrollBar && !this._floating)
                this._viewSize.x -= this._vtScrollBar.width;
            this._viewSize.x -= (this._owner.margin.left + this._owner.margin.right);
            this._viewSize.y -= (this._owner.margin.top + this._owner.margin.bottom);
            this._viewSize.x = Math.max(1, this._viewSize.x);
            this._viewSize.y = Math.max(1, this._viewSize.y);
            this._pageSize.x = this._viewSize.x;
            this._pageSize.y = this._viewSize.y;
            this.handleSizeChanged();
        }
        setContentSize(aWidth, aHeight) {
            if (this._contentSize.x == aWidth && this._contentSize.y == aHeight)
                return;
            this._contentSize.x = aWidth;
            this._contentSize.y = aHeight;
            this.handleSizeChanged();
        }
        changeContentSizeOnScrolling(deltaWidth, deltaHeight, deltaPosX, deltaPosY) {
            var isRightmost = this._xPos == this._overlapSize.x;
            var isBottom = this._yPos == this._overlapSize.y;
            this._contentSize.x += deltaWidth;
            this._contentSize.y += deltaHeight;
            this.handleSizeChanged();
            if (this._tweening == 1) {
                //如果原来滚动位置是贴边，加入处理继续贴边。
                if (deltaWidth != 0 && isRightmost && this._tweenChange.x < 0) {
                    this._xPos = this._overlapSize.x;
                    this._tweenChange.x = -this._xPos - this._tweenStart.x;
                }
                if (deltaHeight != 0 && isBottom && this._tweenChange.y < 0) {
                    this._yPos = this._overlapSize.y;
                    this._tweenChange.y = -this._yPos - this._tweenStart.y;
                }
            }
            else if (this._tweening == 2) {
                //重新调整起始位置，确保能够顺滑滚下去
                if (deltaPosX != 0) {
                    this._container.x -= deltaPosX;
                    this._tweenStart.x -= deltaPosX;
                    this._xPos = -this._container.x;
                }
                if (deltaPosY != 0) {
                    this._container.y -= deltaPosY;
                    this._tweenStart.y -= deltaPosY;
                    this._yPos = -this._container.y;
                }
            }
            else if (this._dragged) {
                if (deltaPosX != 0) {
                    this._container.x -= deltaPosX;
                    this._containerPos.x -= deltaPosX;
                    this._xPos = -this._container.x;
                }
                if (deltaPosY != 0) {
                    this._container.y -= deltaPosY;
                    this._containerPos.y -= deltaPosY;
                    this._yPos = -this._container.y;
                }
            }
            else {
                //如果原来滚动位置是贴边，加入处理继续贴边。
                if (deltaWidth != 0 && isRightmost) {
                    this._xPos = this._overlapSize.x;
                    this._container.x = -this._xPos;
                }
                if (deltaHeight != 0 && isBottom) {
                    this._yPos = this._overlapSize.y;
                    this._container.y = -this._yPos;
                }
            }
            if (this._pageMode)
                this.updatePageController();
        }
        handleSizeChanged() {
            if (this._displayInDemand) {
                this._vScrollNone = this._contentSize.y <= this._viewSize.y;
                this._hScrollNone = this._contentSize.x <= this._viewSize.x;
            }
            if (this._vtScrollBar) {
                if (this._contentSize.y == 0)
                    this._vtScrollBar.setDisplayPerc(0);
                else
                    this._vtScrollBar.setDisplayPerc(Math.min(1, this._viewSize.y / this._contentSize.y));
            }
            if (this._hzScrollBar) {
                if (this._contentSize.x == 0)
                    this._hzScrollBar.setDisplayPerc(0);
                else
                    this._hzScrollBar.setDisplayPerc(Math.min(1, this._viewSize.x / this._contentSize.x));
            }
            this.updateScrollBarVisible();
            var rect = this._maskContainer.scrollRect;
            if (rect) {
                rect.width = this._viewSize.x;
                rect.height = this._viewSize.y;
                if (this._vScrollNone && this._vtScrollBar)
                    rect.width += this._vtScrollBar.width;
                if (this._hScrollNone && this._hzScrollBar)
                    rect.height += this._hzScrollBar.height;
                if (this._dontClipMargin) {
                    rect.width += (this._owner.margin.left + this._owner.margin.right);
                    rect.height += (this._owner.margin.top + this._owner.margin.bottom);
                }
                this._maskContainer.scrollRect = rect;
            }
            if (this._scrollType == fgui.ScrollType.Horizontal || this._scrollType == fgui.ScrollType.Both)
                this._overlapSize.x = Math.ceil(Math.max(0, this._contentSize.x - this._viewSize.x));
            else
                this._overlapSize.x = 0;
            if (this._scrollType == fgui.ScrollType.Vertical || this._scrollType == fgui.ScrollType.Both)
                this._overlapSize.y = Math.ceil(Math.max(0, this._contentSize.y - this._viewSize.y));
            else
                this._overlapSize.y = 0;
            //边界检查
            this._xPos = fgui.ToolSet.clamp(this._xPos, 0, this._overlapSize.x);
            this._yPos = fgui.ToolSet.clamp(this._yPos, 0, this._overlapSize.y);
            if (this._refreshBarAxis != null) {
                var max = this._overlapSize[this._refreshBarAxis];
                if (max == 0)
                    max = Math.max(this._contentSize[this._refreshBarAxis] + this._footerLockedSize - this._viewSize[this._refreshBarAxis], 0);
                else
                    max += this._footerLockedSize;
                if (this._refreshBarAxis == "x") {
                    this._container.pos(fgui.ToolSet.clamp(this._container.x, -max, this._headerLockedSize), fgui.ToolSet.clamp(this._container.y, -this._overlapSize.y, 0));
                }
                else {
                    this._container.pos(fgui.ToolSet.clamp(this._container.x, -this._overlapSize.x, 0), fgui.ToolSet.clamp(this._container.y, -max, this._headerLockedSize));
                }
                if (this._header) {
                    if (this._refreshBarAxis == "x")
                        this._header.height = this._viewSize.y;
                    else
                        this._header.width = this._viewSize.x;
                }
                if (this._footer) {
                    if (this._refreshBarAxis == "y")
                        this._footer.height = this._viewSize.y;
                    else
                        this._footer.width = this._viewSize.x;
                }
            }
            else {
                this._container.pos(fgui.ToolSet.clamp(this._container.x, -this._overlapSize.x, 0), fgui.ToolSet.clamp(this._container.y, -this._overlapSize.y, 0));
            }
            this.updateScrollBarPos();
            if (this._pageMode)
                this.updatePageController();
        }
        posChanged(ani) {
            if (this._aniFlag == 0)
                this._aniFlag = ani ? 1 : -1;
            else if (this._aniFlag == 1 && !ani)
                this._aniFlag = -1;
            this._needRefresh = true;
            Laya.timer.callLater(this, this.refresh);
        }
        refresh() {
            if (!this._owner.displayObject) {
                return;
            }
            this._needRefresh = false;
            Laya.timer.clear(this, this.refresh);
            if (this._pageMode || this._snapToItem) {
                sEndPos.setTo(-this._xPos, -this._yPos);
                this.alignPosition(sEndPos, false);
                this._xPos = -sEndPos.x;
                this._yPos = -sEndPos.y;
            }
            this.refresh2();
            fgui.Events.dispatch(fgui.Events.SCROLL, this._owner.displayObject);
            if (this._needRefresh) //在onScroll事件里开发者可能修改位置，这里再刷新一次，避免闪烁
             {
                this._needRefresh = false;
                Laya.timer.clear(this, this.refresh);
                this.refresh2();
            }
            this.updateScrollBarPos();
            this._aniFlag = 0;
        }
        refresh2() {
            if (this._aniFlag == 1 && !this._dragged) {
                var posX;
                var posY;
                if (this._overlapSize.x > 0)
                    posX = -Math.floor(this._xPos);
                else {
                    if (this._container.x != 0)
                        this._container.x = 0;
                    posX = 0;
                }
                if (this._overlapSize.y > 0)
                    posY = -Math.floor(this._yPos);
                else {
                    if (this._container.y != 0)
                        this._container.y = 0;
                    posY = 0;
                }
                if (posX != this._container.x || posY != this._container.y) {
                    this._tweenDuration.setTo(TWEEN_TIME_GO, TWEEN_TIME_GO);
                    this._tweenStart.setTo(this._container.x, this._container.y);
                    this._tweenChange.setTo(posX - this._tweenStart.x, posY - this._tweenStart.y);
                    this.startTween(1);
                }
                else if (this._tweening != 0)
                    this.killTween();
            }
            else {
                if (this._tweening != 0)
                    this.killTween();
                this._container.pos(Math.floor(-this._xPos), Math.floor(-this._yPos));
                this.loopCheckingCurrent();
            }
            if (this._pageMode)
                this.updatePageController();
        }
        __mouseDown() {
            if (!this._touchEffect)
                return;
            if (this._tweening != 0) {
                this.killTween();
                this._dragged = true;
            }
            else
                this._dragged = false;
            var pt = this._owner.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY, s_vec2);
            this._containerPos.setTo(this._container.x, this._container.y);
            this._beginTouchPos.setTo(pt.x, pt.y);
            this._lastTouchPos.setTo(pt.x, pt.y);
            this._lastTouchGlobalPos.setTo(Laya.stage.mouseX, Laya.stage.mouseY);
            this._isHoldAreaDone = false;
            this._velocity.setTo(0, 0);
            this._velocityScale = 1;
            this._lastMoveTime = Laya.timer.currTimer / 1000;
            this._owner.displayObject.stage.on(Laya.Event.MOUSE_MOVE, this, this.__mouseMove);
            this._owner.displayObject.stage.on(Laya.Event.MOUSE_UP, this, this.__mouseUp);
            this._owner.displayObject.stage.on(Laya.Event.CLICK, this, this.__click);
        }
        __mouseMove() {
            if (!this._touchEffect || this.owner.isDisposed)
                return;
            if (ScrollPane.draggingPane && ScrollPane.draggingPane != this || fgui.GObject.draggingObject) //已经有其他拖动
                return;
            var sensitivity = fgui.UIConfig.touchScrollSensitivity;
            var pt = this._owner.globalToLocal(Laya.stage.mouseX, Laya.stage.mouseY, s_vec2);
            var diff, diff2;
            var sv, sh, st;
            if (this._scrollType == fgui.ScrollType.Vertical) {
                if (!this._isHoldAreaDone) {
                    //表示正在监测垂直方向的手势
                    _gestureFlag |= 1;
                    diff = Math.abs(this._beginTouchPos.y - pt.y);
                    if (diff < sensitivity)
                        return;
                    if ((_gestureFlag & 2) != 0) //已经有水平方向的手势在监测，那么我们用严格的方式检查是不是按垂直方向移动，避免冲突
                     {
                        diff2 = Math.abs(this._beginTouchPos.x - pt.x);
                        if (diff < diff2) //不通过则不允许滚动了
                            return;
                    }
                }
                sv = true;
            }
            else if (this._scrollType == fgui.ScrollType.Horizontal) {
                if (!this._isHoldAreaDone) {
                    _gestureFlag |= 2;
                    diff = Math.abs(this._beginTouchPos.x - pt.x);
                    if (diff < sensitivity)
                        return;
                    if ((_gestureFlag & 1) != 0) {
                        diff2 = Math.abs(this._beginTouchPos.y - pt.y);
                        if (diff < diff2)
                            return;
                    }
                }
                sh = true;
            }
            else {
                _gestureFlag = 3;
                if (!this._isHoldAreaDone) {
                    diff = Math.abs(this._beginTouchPos.y - pt.y);
                    if (diff < sensitivity) {
                        diff = Math.abs(this._beginTouchPos.x - pt.x);
                        if (diff < sensitivity)
                            return;
                    }
                }
                sv = sh = true;
            }
            var newPosX = Math.floor(this._containerPos.x + pt.x - this._beginTouchPos.x);
            var newPosY = Math.floor(this._containerPos.y + pt.y - this._beginTouchPos.y);
            if (sv) {
                if (newPosY > 0) {
                    if (!this._bouncebackEffect)
                        this._container.y = 0;
                    else if (this._header && this._header.maxHeight != 0)
                        this._container.y = Math.floor(Math.min(newPosY * 0.5, this._header.maxHeight));
                    else
                        this._container.y = Math.floor(Math.min(newPosY * 0.5, this._viewSize.y * PULL_RATIO));
                }
                else if (newPosY < -this._overlapSize.y) {
                    if (!this._bouncebackEffect)
                        this._container.y = -this._overlapSize.y;
                    else if (this._footer && this._footer.maxHeight > 0)
                        this._container.y = Math.floor(Math.max((newPosY + this._overlapSize.y) * 0.5, -this._footer.maxHeight) - this._overlapSize.y);
                    else
                        this._container.y = Math.floor(Math.max((newPosY + this._overlapSize.y) * 0.5, -this._viewSize.y * PULL_RATIO) - this._overlapSize.y);
                }
                else
                    this._container.y = newPosY;
            }
            if (sh) {
                if (newPosX > 0) {
                    if (!this._bouncebackEffect)
                        this._container.x = 0;
                    else if (this._header && this._header.maxWidth != 0)
                        this._container.x = Math.floor(Math.min(newPosX * 0.5, this._header.maxWidth));
                    else
                        this._container.x = Math.floor(Math.min(newPosX * 0.5, this._viewSize.x * PULL_RATIO));
                }
                else if (newPosX < 0 - this._overlapSize.x) {
                    if (!this._bouncebackEffect)
                        this._container.x = -this._overlapSize.x;
                    else if (this._footer && this._footer.maxWidth > 0)
                        this._container.x = Math.floor(Math.max((newPosX + this._overlapSize.x) * 0.5, -this._footer.maxWidth) - this._overlapSize.x);
                    else
                        this._container.x = Math.floor(Math.max((newPosX + this._overlapSize.x) * 0.5, -this._viewSize.x * PULL_RATIO) - this._overlapSize.x);
                }
                else
                    this._container.x = newPosX;
            }
            //更新速度
            var frameRate = Laya.stage.frameRate == Laya.Stage.FRAME_SLOW ? 30 : 60;
            var now = Laya.timer.currTimer / 1000;
            var deltaTime = Math.max(now - this._lastMoveTime, 1 / frameRate);
            var deltaPositionX = pt.x - this._lastTouchPos.x;
            var deltaPositionY = pt.y - this._lastTouchPos.y;
            if (!sh)
                deltaPositionX = 0;
            if (!sv)
                deltaPositionY = 0;
            if (deltaTime != 0) {
                var elapsed = deltaTime * frameRate - 1;
                if (elapsed > 1) //速度衰减
                 {
                    var factor = Math.pow(0.833, elapsed);
                    this._velocity.x = this._velocity.x * factor;
                    this._velocity.y = this._velocity.y * factor;
                }
                this._velocity.x = fgui.ToolSet.lerp(this._velocity.x, deltaPositionX * 60 / frameRate / deltaTime, deltaTime * 10);
                this._velocity.y = fgui.ToolSet.lerp(this._velocity.y, deltaPositionY * 60 / frameRate / deltaTime, deltaTime * 10);
            }
            /*速度计算使用的是本地位移，但在后续的惯性滚动判断中需要用到屏幕位移，所以这里要记录一个位移的比例。
            */
            var deltaGlobalPositionX = this._lastTouchGlobalPos.x - Laya.stage.mouseX;
            var deltaGlobalPositionY = this._lastTouchGlobalPos.y - Laya.stage.mouseY;
            if (deltaPositionX != 0)
                this._velocityScale = Math.abs(deltaGlobalPositionX / deltaPositionX);
            else if (deltaPositionY != 0)
                this._velocityScale = Math.abs(deltaGlobalPositionY / deltaPositionY);
            this._lastTouchPos.setTo(pt.x, pt.y);
            this._lastTouchGlobalPos.setTo(Laya.stage.mouseX, Laya.stage.mouseY);
            this._lastMoveTime = now;
            //同步更新pos值
            if (this._overlapSize.x > 0)
                this._xPos = fgui.ToolSet.clamp(-this._container.x, 0, this._overlapSize.x);
            if (this._overlapSize.y > 0)
                this._yPos = fgui.ToolSet.clamp(-this._container.y, 0, this._overlapSize.y);
            //循环滚动特别检查
            if (this._loop != 0) {
                newPosX = this._container.x;
                newPosY = this._container.y;
                if (this.loopCheckingCurrent()) {
                    this._containerPos.x += this._container.x - newPosX;
                    this._containerPos.y += this._container.y - newPosY;
                }
            }
            ScrollPane.draggingPane = this;
            this._isHoldAreaDone = true;
            this._dragged = true;
            this._maskContainer.mouseEnabled = false;
            this.updateScrollBarPos();
            this.updateScrollBarVisible();
            if (this._pageMode)
                this.updatePageController();
            fgui.Events.dispatch(fgui.Events.SCROLL, this._owner.displayObject);
        }
        __mouseUp() {
            if (this._owner.isDisposed)
                return;
            this._owner.displayObject.stage.off(Laya.Event.MOUSE_MOVE, this, this.__mouseMove);
            this._owner.displayObject.stage.off(Laya.Event.MOUSE_UP, this, this.__mouseUp);
            this._owner.displayObject.stage.off(Laya.Event.CLICK, this, this.__click);
            if (ScrollPane.draggingPane == this)
                ScrollPane.draggingPane = null;
            _gestureFlag = 0;
            if (!this._dragged || !this._touchEffect) {
                this._dragged = false;
                this._maskContainer.mouseEnabled = true;
                return;
            }
            this._dragged = false;
            this._maskContainer.mouseEnabled = true;
            this._tweenStart.setTo(this._container.x, this._container.y);
            sEndPos.setTo(this._tweenStart.x, this._tweenStart.y);
            var flag = false;
            if (this._container.x > 0) {
                sEndPos.x = 0;
                flag = true;
            }
            else if (this._container.x < -this._overlapSize.x) {
                sEndPos.x = -this._overlapSize.x;
                flag = true;
            }
            if (this._container.y > 0) {
                sEndPos.y = 0;
                flag = true;
            }
            else if (this._container.y < -this._overlapSize.y) {
                sEndPos.y = -this._overlapSize.y;
                flag = true;
            }
            if (flag) {
                this._tweenChange.setTo(sEndPos.x - this._tweenStart.x, sEndPos.y - this._tweenStart.y);
                if (this._tweenChange.x < -fgui.UIConfig.touchDragSensitivity || this._tweenChange.y < -fgui.UIConfig.touchDragSensitivity) {
                    this._refreshEventDispatching = true;
                    fgui.Events.dispatch(fgui.Events.PULL_DOWN_RELEASE, this._owner.displayObject);
                    this._refreshEventDispatching = false;
                }
                else if (this._tweenChange.x > fgui.UIConfig.touchDragSensitivity || this._tweenChange.y > fgui.UIConfig.touchDragSensitivity) {
                    this._refreshEventDispatching = true;
                    fgui.Events.dispatch(fgui.Events.PULL_UP_RELEASE, this._owner.displayObject);
                    this._refreshEventDispatching = false;
                }
                if (this._headerLockedSize > 0 && sEndPos[this._refreshBarAxis] == 0) {
                    sEndPos[this._refreshBarAxis] = this._headerLockedSize;
                    this._tweenChange.x = sEndPos.x - this._tweenStart.x;
                    this._tweenChange.y = sEndPos.y - this._tweenStart.y;
                }
                else if (this._footerLockedSize > 0 && sEndPos[this._refreshBarAxis] == -this._overlapSize[this._refreshBarAxis]) {
                    var max = this._overlapSize[this._refreshBarAxis];
                    if (max == 0)
                        max = Math.max(this._contentSize[this._refreshBarAxis] + this._footerLockedSize - this._viewSize[this._refreshBarAxis], 0);
                    else
                        max += this._footerLockedSize;
                    sEndPos[this._refreshBarAxis] = -max;
                    this._tweenChange.x = sEndPos.x - this._tweenStart.x;
                    this._tweenChange.y = sEndPos.y - this._tweenStart.y;
                }
                this._tweenDuration.setTo(TWEEN_TIME_DEFAULT, TWEEN_TIME_DEFAULT);
            }
            else {
                //更新速度
                if (!this._inertiaDisabled) {
                    var frameRate = Laya.stage.frameRate == Laya.Stage.FRAME_SLOW ? 30 : 60;
                    var elapsed = (Laya.timer.currTimer / 1000 - this._lastMoveTime) * frameRate - 1;
                    if (elapsed > 1) {
                        var factor = Math.pow(0.833, elapsed);
                        this._velocity.x = this._velocity.x * factor;
                        this._velocity.y = this._velocity.y * factor;
                    }
                    //根据速度计算目标位置和需要时间
                    this.updateTargetAndDuration(this._tweenStart, sEndPos);
                }
                else
                    this._tweenDuration.setTo(TWEEN_TIME_DEFAULT, TWEEN_TIME_DEFAULT);
                sOldChange.setTo(sEndPos.x - this._tweenStart.x, sEndPos.y - this._tweenStart.y);
                //调整目标位置
                this.loopCheckingTarget(sEndPos);
                if (this._pageMode || this._snapToItem)
                    this.alignPosition(sEndPos, true);
                this._tweenChange.x = sEndPos.x - this._tweenStart.x;
                this._tweenChange.y = sEndPos.y - this._tweenStart.y;
                if (this._tweenChange.x == 0 && this._tweenChange.y == 0) {
                    this.updateScrollBarVisible();
                    return;
                }
                //如果目标位置已调整，随之调整需要时间
                if (this._pageMode || this._snapToItem) {
                    this.fixDuration("x", sOldChange.x);
                    this.fixDuration("y", sOldChange.y);
                }
            }
            this.startTween(2);
        }
        __click() {
            this._dragged = false;
        }
        __mouseWheel(evt) {
            if (!this._mouseWheelEnabled)
                return;
            let delta = Math.sign(evt.delta);
            if (this._overlapSize.x > 0 && this._overlapSize.y == 0) {
                if (this._pageMode)
                    this.setPosX(this._xPos + this._pageSize.x * delta, false);
                else
                    this.setPosX(this._xPos + this._mouseWheelStep * delta, false);
            }
            else {
                if (this._pageMode)
                    this.setPosY(this._yPos + this._pageSize.y * delta, false);
                else
                    this.setPosY(this._yPos + this._mouseWheelStep * delta, false);
            }
        }
        updateScrollBarPos() {
            if (this._vtScrollBar)
                this._vtScrollBar.setScrollPerc(this._overlapSize.y == 0 ? 0 : fgui.ToolSet.clamp(-this._container.y, 0, this._overlapSize.y) / this._overlapSize.y);
            if (this._hzScrollBar)
                this._hzScrollBar.setScrollPerc(this._overlapSize.x == 0 ? 0 : fgui.ToolSet.clamp(-this._container.x, 0, this._overlapSize.x) / this._overlapSize.x);
            this.checkRefreshBar();
        }
        updateScrollBarVisible() {
            if (this._vtScrollBar) {
                if (this._viewSize.y <= this._vtScrollBar.minSize || this._vScrollNone)
                    this._vtScrollBar.displayObject.visible = false;
                else
                    this.updateScrollBarVisible2(this._vtScrollBar);
            }
            if (this._hzScrollBar) {
                if (this._viewSize.x <= this._hzScrollBar.minSize || this._hScrollNone)
                    this._hzScrollBar.displayObject.visible = false;
                else
                    this.updateScrollBarVisible2(this._hzScrollBar);
            }
        }
        updateScrollBarVisible2(bar) {
            if (this._scrollBarDisplayAuto)
                fgui.GTween.kill(bar, false, "alpha");
            if (this._scrollBarDisplayAuto && this._tweening == 0 && !this._dragged && !bar.gripDragging) {
                if (bar.displayObject.visible)
                    fgui.GTween.to(1, 0, 0.5).setDelay(0.5).onComplete(this.__barTweenComplete, this).setTarget(bar, "alpha");
            }
            else {
                bar.alpha = 1;
                bar.displayObject.visible = true;
            }
        }
        __barTweenComplete(tweener) {
            var bar = (tweener.target);
            bar.alpha = 1;
            bar.displayObject.visible = false;
        }
        getLoopPartSize(division, axis) {
            return (this._contentSize[axis] + (axis == "x" ? (this._owner).columnGap : (this._owner).lineGap)) / division;
        }
        loopCheckingCurrent() {
            var changed = false;
            if (this._loop == 1 && this._overlapSize.x > 0) {
                if (this._xPos < 0.001) {
                    this._xPos += this.getLoopPartSize(2, "x");
                    changed = true;
                }
                else if (this._xPos >= this._overlapSize.x) {
                    this._xPos -= this.getLoopPartSize(2, "x");
                    changed = true;
                }
            }
            else if (this._loop == 2 && this._overlapSize.y > 0) {
                if (this._yPos < 0.001) {
                    this._yPos += this.getLoopPartSize(2, "y");
                    changed = true;
                }
                else if (this._yPos >= this._overlapSize.y) {
                    this._yPos -= this.getLoopPartSize(2, "y");
                    changed = true;
                }
            }
            if (changed)
                this._container.pos(Math.floor(-this._xPos), Math.floor(-this._yPos));
            return changed;
        }
        loopCheckingTarget(endPos) {
            if (this._loop == 1)
                this.loopCheckingTarget2(endPos, "x");
            if (this._loop == 2)
                this.loopCheckingTarget2(endPos, "y");
        }
        loopCheckingTarget2(endPos, axis) {
            var halfSize;
            var tmp;
            if (endPos[axis] > 0) {
                halfSize = this.getLoopPartSize(2, axis);
                tmp = this._tweenStart[axis] - halfSize;
                if (tmp <= 0 && tmp >= -this._overlapSize[axis]) {
                    endPos[axis] -= halfSize;
                    this._tweenStart[axis] = tmp;
                }
            }
            else if (endPos[axis] < -this._overlapSize[axis]) {
                halfSize = this.getLoopPartSize(2, axis);
                tmp = this._tweenStart[axis] + halfSize;
                if (tmp <= 0 && tmp >= -this._overlapSize[axis]) {
                    endPos[axis] += halfSize;
                    this._tweenStart[axis] = tmp;
                }
            }
        }
        loopCheckingNewPos(value, axis) {
            if (this._overlapSize[axis] == 0)
                return value;
            var pos = axis == "x" ? this._xPos : this._yPos;
            var changed = false;
            var v;
            if (value < 0.001) {
                value += this.getLoopPartSize(2, axis);
                if (value > pos) {
                    v = this.getLoopPartSize(6, axis);
                    v = Math.ceil((value - pos) / v) * v;
                    pos = fgui.ToolSet.clamp(pos + v, 0, this._overlapSize[axis]);
                    changed = true;
                }
            }
            else if (value >= this._overlapSize[axis]) {
                value -= this.getLoopPartSize(2, axis);
                if (value < pos) {
                    v = this.getLoopPartSize(6, axis);
                    v = Math.ceil((pos - value) / v) * v;
                    pos = fgui.ToolSet.clamp(pos - v, 0, this._overlapSize[axis]);
                    changed = true;
                }
            }
            if (changed) {
                if (axis == "x")
                    this._container.x = -Math.floor(pos);
                else
                    this._container.y = -Math.floor(pos);
            }
            return value;
        }
        alignPosition(pos, inertialScrolling) {
            if (this._pageMode) {
                pos.x = this.alignByPage(pos.x, "x", inertialScrolling);
                pos.y = this.alignByPage(pos.y, "y", inertialScrolling);
            }
            else if (this._snapToItem) {
                var xDir = 0;
                var yDir = 0;
                if (inertialScrolling) {
                    xDir = pos.x - this._containerPos.x;
                    yDir = pos.y - this._containerPos.y;
                }
                var pt = this._owner.getSnappingPositionWithDir(-pos.x, -pos.y, xDir, yDir, s_vec2);
                if (pos.x < 0 && pos.x > -this._overlapSize.x)
                    pos.x = -pt.x;
                if (pos.y < 0 && pos.y > -this._overlapSize.y)
                    pos.y = -pt.y;
            }
        }
        alignByPage(pos, axis, inertialScrolling) {
            var page;
            if (pos > 0)
                page = 0;
            else if (pos < -this._overlapSize[axis])
                page = Math.ceil(this._contentSize[axis] / this._pageSize[axis]) - 1;
            else {
                page = Math.floor(-pos / this._pageSize[axis]);
                var change = inertialScrolling ? (pos - this._containerPos[axis]) : (pos - this._container[axis]);
                var testPageSize = Math.min(this._pageSize[axis], this._contentSize[axis] - (page + 1) * this._pageSize[axis]);
                var delta = -pos - page * this._pageSize[axis];
                //页面吸附策略
                if (Math.abs(change) > this._pageSize[axis]) //如果滚动距离超过1页,则需要超过页面的一半，才能到更下一页
                 {
                    if (delta > testPageSize * 0.5)
                        page++;
                }
                else //否则只需要页面的1/3，当然，需要考虑到左移和右移的情况
                 {
                    if (delta > testPageSize * (change < 0 ? fgui.UIConfig.defaultScrollPagingThreshold : (1 - fgui.UIConfig.defaultScrollPagingThreshold)))
                        page++;
                }
                //重新计算终点
                pos = -page * this._pageSize[axis];
                if (pos < -this._overlapSize[axis]) //最后一页未必有pageSize那么大
                    pos = -this._overlapSize[axis];
            }
            //惯性滚动模式下，会增加判断尽量不要滚动超过一页
            if (inertialScrolling) {
                var oldPos = this._tweenStart[axis];
                var oldPage;
                if (oldPos > 0)
                    oldPage = 0;
                else if (oldPos < -this._overlapSize[axis])
                    oldPage = Math.ceil(this._contentSize[axis] / this._pageSize[axis]) - 1;
                else
                    oldPage = Math.floor(-oldPos / this._pageSize[axis]);
                var startPage = Math.floor(-this._containerPos[axis] / this._pageSize[axis]);
                if (Math.abs(page - startPage) > 1 && Math.abs(oldPage - startPage) <= 1) {
                    if (page > startPage)
                        page = startPage + 1;
                    else
                        page = startPage - 1;
                    pos = -page * this._pageSize[axis];
                }
            }
            return pos;
        }
        updateTargetAndDuration(orignPos, resultPos) {
            resultPos.x = this.updateTargetAndDuration2(orignPos.x, "x");
            resultPos.y = this.updateTargetAndDuration2(orignPos.y, "y");
        }
        updateTargetAndDuration2(pos, axis) {
            var v = this._velocity[axis];
            var duration = 0;
            if (pos > 0)
                pos = 0;
            else if (pos < -this._overlapSize[axis])
                pos = -this._overlapSize[axis];
            else {
                //以屏幕像素为基准
                var v2 = Math.abs(v) * this._velocityScale;
                //在移动设备上，需要对不同分辨率做一个适配，我们的速度判断以1136分辨率为基准
                if (Laya.Browser.onMobile)
                    v2 *= 1136 / Math.max(Laya.stage.width, Laya.stage.height);
                //这里有一些阈值的处理，因为在低速内，不希望产生较大的滚动（甚至不滚动）
                var ratio = 0;
                if (this._pageMode || !Laya.Browser.onMobile) {
                    if (v2 > 500)
                        ratio = Math.pow((v2 - 500) / 500, 2);
                }
                else {
                    if (v2 > 1000)
                        ratio = Math.pow((v2 - 1000) / 1000, 2);
                }
                if (ratio != 0) {
                    if (ratio > 1)
                        ratio = 1;
                    v2 *= ratio;
                    v *= ratio;
                    this._velocity[axis] = v;
                    //算法：v*（_decelerationRate的n次幂）= 60，即在n帧后速度降为60（假设每秒60帧）。
                    duration = Math.log(60 / v2) / Math.log(this._decelerationRate) / 60;
                    //计算距离要使用本地速度
                    //理论公式貌似滚动的距离不够，改为经验公式
                    //var change:number = (v/ 60 - 1) / (1 - this._decelerationRate);
                    var change = Math.floor(v * duration * 0.4);
                    pos += change;
                }
            }
            if (duration < TWEEN_TIME_DEFAULT)
                duration = TWEEN_TIME_DEFAULT;
            this._tweenDuration[axis] = duration;
            return pos;
        }
        fixDuration(axis, oldChange) {
            if (this._tweenChange[axis] == 0 || Math.abs(this._tweenChange[axis]) >= Math.abs(oldChange))
                return;
            var newDuration = Math.abs(this._tweenChange[axis] / oldChange) * this._tweenDuration[axis];
            if (newDuration < TWEEN_TIME_DEFAULT)
                newDuration = TWEEN_TIME_DEFAULT;
            this._tweenDuration[axis] = newDuration;
        }
        startTween(type) {
            this._tweenTime.setTo(0, 0);
            this._tweening = type;
            Laya.timer.frameLoop(1, this, this.tweenUpdate);
            this.updateScrollBarVisible();
        }
        killTween() {
            if (this._tweening == 1) //取消类型为1的tween需立刻设置到终点
             {
                this._container.pos(this._tweenStart.x + this._tweenChange.x, this._tweenStart.y + this._tweenChange.y);
                fgui.Events.dispatch(fgui.Events.SCROLL, this._owner.displayObject);
            }
            this._tweening = 0;
            Laya.timer.clear(this, this.tweenUpdate);
            this.updateScrollBarVisible();
            fgui.Events.dispatch(fgui.Events.SCROLL_END, this._owner.displayObject);
        }
        checkRefreshBar() {
            if (!this._header && !this._footer)
                return;
            var pos = this._container[this._refreshBarAxis];
            if (this._header) {
                if (pos > 0) {
                    if (!this._header.displayObject.parent)
                        this._maskContainer.addChildAt(this._header.displayObject, 0);
                    var pt = s_vec2;
                    pt.setTo(this._header.width, this._header.height);
                    pt[this._refreshBarAxis] = pos;
                    this._header.setSize(pt.x, pt.y);
                }
                else {
                    if (this._header.displayObject.parent)
                        this._maskContainer.removeChild(this._header.displayObject);
                }
            }
            if (this._footer) {
                var max = this._overlapSize[this._refreshBarAxis];
                if (pos < -max || max == 0 && this._footerLockedSize > 0) {
                    if (!this._footer.displayObject.parent)
                        this._maskContainer.addChildAt(this._footer.displayObject, 0);
                    pt = s_vec2;
                    pt.setTo(this._footer.x, this._footer.y);
                    if (max > 0)
                        pt[this._refreshBarAxis] = pos + this._contentSize[this._refreshBarAxis];
                    else
                        pt[this._refreshBarAxis] = Math.max(Math.min(pos + this._viewSize[this._refreshBarAxis], this._viewSize[this._refreshBarAxis] - this._footerLockedSize), this._viewSize[this._refreshBarAxis] - this._contentSize[this._refreshBarAxis]);
                    this._footer.setXY(pt.x, pt.y);
                    pt.setTo(this._footer.width, this._footer.height);
                    if (max > 0)
                        pt[this._refreshBarAxis] = -max - pos;
                    else
                        pt[this._refreshBarAxis] = this._viewSize[this._refreshBarAxis] - this._footer[this._refreshBarAxis];
                    this._footer.setSize(pt.x, pt.y);
                }
                else {
                    if (this._footer.displayObject.parent)
                        this._maskContainer.removeChild(this._footer.displayObject);
                }
            }
        }
        tweenUpdate() {
            var nx = this.runTween("x");
            var ny = this.runTween("y");
            this._container.pos(nx, ny);
            if (this._tweening == 2) {
                if (this._overlapSize.x > 0)
                    this._xPos = fgui.ToolSet.clamp(-nx, 0, this._overlapSize.x);
                if (this._overlapSize.y > 0)
                    this._yPos = fgui.ToolSet.clamp(-ny, 0, this._overlapSize.y);
                if (this._pageMode)
                    this.updatePageController();
            }
            if (this._tweenChange.x == 0 && this._tweenChange.y == 0) {
                this._tweening = 0;
                Laya.timer.clear(this, this.tweenUpdate);
                this.loopCheckingCurrent();
                this.updateScrollBarPos();
                this.updateScrollBarVisible();
                fgui.Events.dispatch(fgui.Events.SCROLL, this._owner.displayObject);
                fgui.Events.dispatch(fgui.Events.SCROLL_END, this._owner.displayObject);
            }
            else {
                this.updateScrollBarPos();
                fgui.Events.dispatch(fgui.Events.SCROLL, this._owner.displayObject);
            }
        }
        runTween(axis) {
            var newValue;
            if (this._tweenChange[axis] != 0) {
                this._tweenTime[axis] += Laya.timer.delta / 1000;
                if (this._tweenTime[axis] >= this._tweenDuration[axis]) {
                    newValue = this._tweenStart[axis] + this._tweenChange[axis];
                    this._tweenChange[axis] = 0;
                }
                else {
                    var ratio = easeFunc(this._tweenTime[axis], this._tweenDuration[axis]);
                    newValue = this._tweenStart[axis] + Math.floor(this._tweenChange[axis] * ratio);
                }
                var threshold1 = 0;
                var threshold2 = -this._overlapSize[axis];
                if (this._headerLockedSize > 0 && this._refreshBarAxis == axis)
                    threshold1 = this._headerLockedSize;
                if (this._footerLockedSize > 0 && this._refreshBarAxis == axis) {
                    var max = this._overlapSize[this._refreshBarAxis];
                    if (max == 0)
                        max = Math.max(this._contentSize[this._refreshBarAxis] + this._footerLockedSize - this._viewSize[this._refreshBarAxis], 0);
                    else
                        max += this._footerLockedSize;
                    threshold2 = -max;
                }
                if (this._tweening == 2 && this._bouncebackEffect) {
                    if (newValue > 20 + threshold1 && this._tweenChange[axis] > 0
                        || newValue > threshold1 && this._tweenChange[axis] == 0) //开始回弹
                     {
                        this._tweenTime[axis] = 0;
                        this._tweenDuration[axis] = TWEEN_TIME_DEFAULT;
                        this._tweenChange[axis] = -newValue + threshold1;
                        this._tweenStart[axis] = newValue;
                    }
                    else if (newValue < threshold2 - 20 && this._tweenChange[axis] < 0
                        || newValue < threshold2 && this._tweenChange[axis] == 0) //开始回弹
                     {
                        this._tweenTime[axis] = 0;
                        this._tweenDuration[axis] = TWEEN_TIME_DEFAULT;
                        this._tweenChange[axis] = threshold2 - newValue;
                        this._tweenStart[axis] = newValue;
                    }
                }
                else {
                    if (newValue > threshold1) {
                        newValue = threshold1;
                        this._tweenChange[axis] = 0;
                    }
                    else if (newValue < threshold2) {
                        newValue = threshold2;
                        this._tweenChange[axis] = 0;
                    }
                }
            }
            else
                newValue = this._container[axis];
            return newValue;
        }
    }
    fgui.ScrollPane = ScrollPane;
    var _gestureFlag = 0;
    const TWEEN_TIME_GO = 0.5; //调用SetPos(ani)时使用的缓动时间
    const TWEEN_TIME_DEFAULT = 0.3; //惯性滚动的最小缓动时间
    const PULL_RATIO = 0.5; //下拉过顶或者上拉过底时允许超过的距离占显示区域的比例
    var s_vec2 = new Laya.Point();
    var s_rect = new Laya.Rectangle();
    var sEndPos = new Laya.Point();
    var sOldChange = new Laya.Point();
    function easeFunc(t, d) {
        return (t = t / d - 1) * t * t + 1; //cubicOut
    }
})(fgui);

(function (fgui) {
    class Transition {
        constructor(owner) {
            this._owner = owner;
            this._items = new Array();
            this._totalDuration = 0;
            this._autoPlayTimes = 1;
            this._autoPlayDelay = 0;
            this._timeScale = 1;
            this._startTime = 0;
            this._endTime = 0;
        }
        play(onComplete, times, delay, startTime, endTime) {
            this._play(onComplete, times, delay, startTime, endTime, false);
        }
        playReverse(onComplete, times, delay, startTime, endTime) {
            this._play(onComplete, 1, delay, startTime, endTime, true);
        }
        changePlayTimes(value) {
            this._totalTimes = value;
        }
        setAutoPlay(value, times, delay) {
            if (times == undefined)
                times = -1;
            if (delay == undefined)
                delay = 0;
            if (this._autoPlay != value) {
                this._autoPlay = value;
                this._autoPlayTimes = times;
                this._autoPlayDelay = delay;
                if (this._autoPlay) {
                    if (this._owner.onStage)
                        this.play(null, null, this._autoPlayTimes, this._autoPlayDelay);
                }
                else {
                    if (!this._owner.onStage)
                        this.stop(false, true);
                }
            }
        }
        _play(onComplete, times, delay, startTime, endTime, reversed) {
            if (times == undefined)
                times = 1;
            if (delay == undefined)
                delay = 0;
            if (startTime == undefined)
                startTime = 0;
            if (endTime == undefined)
                endTime = -1;
            this.stop(true, true);
            this._totalTimes = times;
            this._reversed = reversed;
            this._startTime = startTime;
            this._endTime = endTime;
            this._playing = true;
            this._paused = false;
            this._onComplete = onComplete;
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.target == null) {
                    if (item.targetId)
                        item.target = this._owner.getChildById(item.targetId);
                    else
                        item.target = this._owner;
                }
                else if (item.target != this._owner && item.target.parent != this._owner)
                    item.target = null;
                if (item.target && item.type == ActionType.Transition) {
                    var trans = item.target.getTransition(item.value.transName);
                    if (trans == this)
                        trans = null;
                    if (trans) {
                        if (item.value.playTimes == 0) //this.stop
                         {
                            var j;
                            for (j = i - 1; j >= 0; j--) {
                                var item2 = this._items[j];
                                if (item2.type == ActionType.Transition) {
                                    if (item2.value.trans == trans) {
                                        item2.value.stopTime = item.time - item2.time;
                                        break;
                                    }
                                }
                            }
                            if (j < 0)
                                item.value.stopTime = 0;
                            else
                                trans = null; //no need to handle this.stop anymore
                        }
                        else
                            item.value.stopTime = -1;
                    }
                    item.value.trans = trans;
                }
            }
            if (delay == 0)
                this.onDelayedPlay();
            else
                fgui.GTween.delayedCall(delay).setTarget(this).onComplete(this.onDelayedPlay, this);
        }
        stop(setToComplete, processCallback) {
            if (!this._playing)
                return;
            if (setToComplete == undefined)
                setToComplete = true;
            this._playing = false;
            this._totalTasks = 0;
            this._totalTimes = 0;
            var handler = this._onComplete;
            this._onComplete = null;
            fgui.GTween.kill(this); //delay start
            var cnt = this._items.length;
            if (this._reversed) {
                for (var i = cnt - 1; i >= 0; i--) {
                    var item = this._items[i];
                    if (item.target == null)
                        continue;
                    this.stopItem(item, setToComplete);
                }
            }
            else {
                for (i = 0; i < cnt; i++) {
                    item = this._items[i];
                    if (item.target == null)
                        continue;
                    this.stopItem(item, setToComplete);
                }
            }
            if (processCallback && handler) {
                typeof handler === 'function' ? handler() : handler.run();
            }
        }
        stopItem(item, setToComplete) {
            if (item.displayLockToken != 0) {
                item.target.releaseDisplayLock(item.displayLockToken);
                item.displayLockToken = 0;
            }
            if (item.tweener) {
                item.tweener.kill(setToComplete);
                item.tweener = null;
                if (item.type == ActionType.Shake && !setToComplete) //震动必须归位，否则下次就越震越远了。
                 {
                    item.target._gearLocked = true;
                    item.target.setXY(item.target.x - item.value.lastOffsetX, item.target.y - item.value.lastOffsetY);
                    item.target._gearLocked = false;
                }
            }
            if (item.type == ActionType.Transition) {
                var trans = item.value.trans;
                if (trans)
                    trans.stop(setToComplete, false);
            }
        }
        setPaused(paused) {
            if (!this._playing || this._paused == paused)
                return;
            this._paused = paused;
            var tweener = fgui.GTween.getTween(this);
            if (tweener)
                tweener.setPaused(paused);
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.target == null)
                    continue;
                if (item.type == ActionType.Transition) {
                    if (item.value.trans)
                        item.value.trans.setPaused(paused);
                }
                else if (item.type == ActionType.Animation) {
                    if (paused) {
                        item.value.flag = item.target.getProp(fgui.ObjectPropID.Playing);
                        item.target.setProp(fgui.ObjectPropID.Playing, false);
                    }
                    else
                        item.target.setProp(fgui.ObjectPropID.Playing, item.value.flag);
                }
                if (item.tweener)
                    item.tweener.setPaused(paused);
            }
        }
        dispose() {
            if (this._playing)
                fgui.GTween.kill(this); //delay start
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.tweener) {
                    item.tweener.kill();
                    item.tweener = null;
                }
                item.target = null;
                item.hook = null;
                if (item.tweenConfig)
                    item.tweenConfig.endHook = null;
            }
            this._items.length = 0;
            this._playing = false;
            this._onComplete = null;
        }
        get playing() {
            return this._playing;
        }
        get totalDuration() {
            return this._totalDuration;
        }
        setValue(label, ...args) {
            var cnt = this._items.length;
            var found = false;
            var value;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.label == label) {
                    if (item.tweenConfig)
                        value = item.tweenConfig.startValue;
                    else
                        value = item.value;
                    found = true;
                }
                else if (item.tweenConfig && item.tweenConfig.endLabel == label) {
                    value = item.tweenConfig.endValue;
                    found = true;
                }
                else
                    continue;
                switch (item.type) {
                    case ActionType.XY:
                    case ActionType.Size:
                    case ActionType.Pivot:
                    case ActionType.Scale:
                    case ActionType.Skew:
                        value.b1 = true;
                        value.b2 = true;
                        value.f1 = parseFloat(args[0]);
                        value.f2 = parseFloat(args[1]);
                        break;
                    case ActionType.Alpha:
                        value.f1 = parseFloat(args[0]);
                        break;
                    case ActionType.Rotation:
                        value.f1 = parseFloat(args[0]);
                        break;
                    case ActionType.Color:
                        value.f1 = parseFloat(args[0]);
                        break;
                    case ActionType.Animation:
                        value.frame = parseInt(args[0]);
                        if (args.length > 1)
                            value.playing = args[1];
                        break;
                    case ActionType.Visible:
                        value.visible = args[0];
                        break;
                    case ActionType.Sound:
                        value.sound = args[0];
                        if (args.length > 1)
                            value.volume = parseFloat(args[1]);
                        break;
                    case ActionType.Transition:
                        value.transName = args[0];
                        if (args.length > 1)
                            value.playTimes = parseInt(args[1]);
                        break;
                    case ActionType.Shake:
                        value.amplitude = parseFloat(args[0]);
                        if (args.length > 1)
                            value.duration = parseFloat(args[1]);
                        break;
                    case ActionType.ColorFilter:
                        value.f1 = parseFloat(args[0]);
                        value.f2 = parseFloat(args[1]);
                        value.f3 = parseFloat(args[2]);
                        value.f4 = parseFloat(args[3]);
                        break;
                    case ActionType.Text:
                    case ActionType.Icon:
                        value.text = args[0];
                        break;
                }
            }
            if (!found)
                throw new Error("this.label not exists");
        }
        setHook(label, callback) {
            var cnt = this._items.length;
            var found = false;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.label == label) {
                    item.hook = callback;
                    found = true;
                    break;
                }
                else if (item.tweenConfig && item.tweenConfig.endLabel == label) {
                    item.tweenConfig.endHook = callback;
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new Error("this.label not exists");
        }
        clearHooks() {
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                item.hook = null;
                if (item.tweenConfig)
                    item.tweenConfig.endHook = null;
            }
        }
        setTarget(label, newTarget) {
            var cnt = this._items.length;
            var found = false;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.label == label) {
                    item.targetId = (newTarget == this._owner || newTarget == null) ? "" : newTarget.id;
                    if (this._playing) {
                        if (item.targetId.length > 0)
                            item.target = this._owner.getChildById(item.targetId);
                        else
                            item.target = this._owner;
                    }
                    else
                        item.target = null;
                    found = true;
                }
            }
            if (!found)
                throw new Error("this.label not exists");
        }
        setDuration(label, value) {
            var cnt = this._items.length;
            var found = false;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.tweenConfig && item.label == label) {
                    item.tweenConfig.duration = value;
                    found = true;
                }
            }
            if (!found)
                throw new Error("this.label not exists");
        }
        getLabelTime(label) {
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.label == label)
                    return item.time;
                else if (item.tweenConfig && item.tweenConfig.endLabel == label)
                    return item.time + item.tweenConfig.duration;
            }
            return NaN;
        }
        get timeScale() {
            return this._timeScale;
        }
        set timeScale(value) {
            if (this._timeScale != value) {
                this._timeScale = value;
                if (this._playing) {
                    var cnt = this._items.length;
                    for (var i = 0; i < cnt; i++) {
                        var item = this._items[i];
                        if (item.tweener)
                            item.tweener.setTimeScale(value);
                        else if (item.type == ActionType.Transition) {
                            if (item.value.trans)
                                item.value.trans.timeScale = value;
                        }
                        else if (item.type == ActionType.Animation) {
                            if (item.target)
                                item.target.setProp(fgui.ObjectPropID.TimeScale, value);
                        }
                    }
                }
            }
        }
        updateFromRelations(targetId, dx, dy) {
            var cnt = this._items.length;
            if (cnt == 0)
                return;
            for (var i = 0; i < cnt; i++) {
                var item = this._items[i];
                if (item.type == ActionType.XY && item.targetId == targetId) {
                    if (item.tweenConfig) {
                        if (!item.tweenConfig.startValue.b3) {
                            item.tweenConfig.startValue.f1 += dx;
                            item.tweenConfig.startValue.f2 += dy;
                        }
                        if (!item.tweenConfig.endValue.b3) {
                            item.tweenConfig.endValue.f1 += dx;
                            item.tweenConfig.endValue.f2 += dy;
                        }
                    }
                    else {
                        if (!item.value.b3) {
                            item.value.f1 += dx;
                            item.value.f2 += dy;
                        }
                    }
                }
            }
        }
        onOwnerAddedToStage() {
            if (this._autoPlay && !this._playing)
                this.play(null, this._autoPlayTimes, this._autoPlayDelay);
        }
        onOwnerRemovedFromStage() {
            if ((this._options & OPTION_AUTO_STOP_DISABLED) == 0)
                this.stop((this._options & OPTION_AUTO_STOP_AT_END) != 0 ? true : false, false);
        }
        onDelayedPlay() {
            this.internalPlay();
            this._playing = this._totalTasks > 0;
            if (this._playing) {
                if ((this._options & OPTION_IGNORE_DISPLAY_CONTROLLER) != 0) {
                    var cnt = this._items.length;
                    for (var i = 0; i < cnt; i++) {
                        var item = this._items[i];
                        if (item.target && item.target != this._owner)
                            item.displayLockToken = item.target.addDisplayLock();
                    }
                }
            }
            else if (this._onComplete) {
                var handler = this._onComplete;
                this._onComplete = null;
                typeof handler === 'function' ? handler() : handler.run();
            }
        }
        internalPlay() {
            this._ownerBaseX = this._owner.x;
            this._ownerBaseY = this._owner.y;
            this._totalTasks = 1;
            var cnt = this._items.length;
            var item;
            var needSkipAnimations = false;
            if (!this._reversed) {
                for (var i = 0; i < cnt; i++) {
                    item = this._items[i];
                    if (item.target == null)
                        continue;
                    if (item.type == ActionType.Animation && this._startTime != 0 && item.time <= this._startTime) {
                        needSkipAnimations = true;
                        item.value.flag = false;
                    }
                    else
                        this.playItem(item);
                }
            }
            else {
                for (i = cnt - 1; i >= 0; i--) {
                    item = this._items[i];
                    if (item.target == null)
                        continue;
                    this.playItem(item);
                }
            }
            if (needSkipAnimations)
                this.skipAnimations();
            this._totalTasks--;
        }
        playItem(item) {
            var time;
            if (item.tweenConfig) {
                if (this._reversed)
                    time = (this._totalDuration - item.time - item.tweenConfig.duration);
                else
                    time = item.time;
                if (this._endTime == -1 || time <= this._endTime) {
                    var startValue;
                    var endValue;
                    if (this._reversed) {
                        startValue = item.tweenConfig.endValue;
                        endValue = item.tweenConfig.startValue;
                    }
                    else {
                        startValue = item.tweenConfig.startValue;
                        endValue = item.tweenConfig.endValue;
                    }
                    item.value.b1 = startValue.b1 || endValue.b1;
                    item.value.b2 = startValue.b2 || endValue.b2;
                    switch (item.type) {
                        case ActionType.XY:
                        case ActionType.Size:
                        case ActionType.Scale:
                        case ActionType.Skew:
                            item.tweener = fgui.GTween.to2(startValue.f1, startValue.f2, endValue.f1, endValue.f2, item.tweenConfig.duration);
                            break;
                        case ActionType.Alpha:
                        case ActionType.Rotation:
                            item.tweener = fgui.GTween.to(startValue.f1, endValue.f1, item.tweenConfig.duration);
                            break;
                        case ActionType.Color:
                            item.tweener = fgui.GTween.toColor(startValue.f1, endValue.f1, item.tweenConfig.duration);
                            break;
                        case ActionType.ColorFilter:
                            item.tweener = fgui.GTween.to4(startValue.f1, startValue.f2, startValue.f3, startValue.f4, endValue.f1, endValue.f2, endValue.f3, endValue.f4, item.tweenConfig.duration);
                            break;
                    }
                    item.tweener.setDelay(time)
                        .setEase(item.tweenConfig.easeType)
                        .setRepeat(item.tweenConfig.repeat, item.tweenConfig.yoyo)
                        .setTimeScale(this._timeScale)
                        .setTarget(item)
                        .onStart(this.onTweenStart, this)
                        .onUpdate(this.onTweenUpdate, this)
                        .onComplete(this.onTweenComplete, this);
                    if (this._endTime >= 0)
                        item.tweener.setBreakpoint(this._endTime - time);
                    this._totalTasks++;
                }
            }
            else if (item.type == ActionType.Shake) {
                if (this._reversed)
                    time = (this._totalDuration - item.time - item.value.duration);
                else
                    time = item.time;
                item.value.offsetX = item.value.offsetY = 0;
                item.value.lastOffsetX = item.value.lastOffsetY = 0;
                item.tweener = fgui.GTween.shake(0, 0, item.value.amplitude, item.value.duration)
                    .setDelay(time)
                    .setTimeScale(this._timeScale)
                    .setTarget(item)
                    .onUpdate(this.onTweenUpdate, this)
                    .onComplete(this.onTweenComplete, this);
                if (this._endTime >= 0)
                    item.tweener.setBreakpoint(this._endTime - item.time);
                this._totalTasks++;
            }
            else {
                if (this._reversed)
                    time = (this._totalDuration - item.time);
                else
                    time = item.time;
                if (time <= this._startTime) {
                    this.applyValue(item);
                    this.callHook(item, false);
                }
                else if (this._endTime == -1 || time <= this._endTime) {
                    this._totalTasks++;
                    item.tweener = fgui.GTween.delayedCall(time)
                        .setTimeScale(this._timeScale)
                        .setTarget(item)
                        .onComplete(this.onDelayedPlayItem, this);
                }
            }
            if (item.tweener)
                item.tweener.seek(this._startTime);
        }
        skipAnimations() {
            var frame;
            var playStartTime;
            var playTotalTime;
            var value;
            var target;
            var item;
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                item = this._items[i];
                if (item.type != ActionType.Animation || item.time > this._startTime)
                    continue;
                value = item.value;
                if (value.flag)
                    continue;
                target = item.target;
                frame = target.getProp(fgui.ObjectPropID.Frame);
                playStartTime = target.getProp(fgui.ObjectPropID.Playing) ? 0 : -1;
                playTotalTime = 0;
                for (var j = i; j < cnt; j++) {
                    item = this._items[j];
                    if (item.type != ActionType.Animation || item.target != target || item.time > this._startTime)
                        continue;
                    value = item.value;
                    value.flag = true;
                    if (value.frame != -1) {
                        frame = value.frame;
                        if (value.playing)
                            playStartTime = item.time;
                        else
                            playStartTime = -1;
                        playTotalTime = 0;
                    }
                    else {
                        if (value.playing) {
                            if (playStartTime < 0)
                                playStartTime = item.time;
                        }
                        else {
                            if (playStartTime >= 0)
                                playTotalTime += (item.time - playStartTime);
                            playStartTime = -1;
                        }
                    }
                    this.callHook(item, false);
                }
                if (playStartTime >= 0)
                    playTotalTime += (this._startTime - playStartTime);
                target.setProp(fgui.ObjectPropID.Playing, playStartTime >= 0);
                target.setProp(fgui.ObjectPropID.Frame, frame);
                if (playTotalTime > 0)
                    target.setProp(fgui.ObjectPropID.DeltaTime, playTotalTime * 1000);
            }
        }
        onDelayedPlayItem(tweener) {
            var item = tweener.target;
            item.tweener = null;
            this._totalTasks--;
            this.applyValue(item);
            this.callHook(item, false);
            this.checkAllComplete();
        }
        onTweenStart(tweener) {
            var item = tweener.target;
            if (item.type == ActionType.XY || item.type == ActionType.Size) //位置和大小要到start才最终确认起始值
             {
                var startValue;
                var endValue;
                if (this._reversed) {
                    startValue = item.tweenConfig.endValue;
                    endValue = item.tweenConfig.startValue;
                }
                else {
                    startValue = item.tweenConfig.startValue;
                    endValue = item.tweenConfig.endValue;
                }
                if (item.type == ActionType.XY) {
                    if (item.target != this._owner) {
                        if (!startValue.b1)
                            tweener.startValue.x = item.target.x;
                        else if (startValue.b3) //percent
                            tweener.startValue.x = startValue.f1 * this._owner.width;
                        if (!startValue.b2)
                            tweener.startValue.y = item.target.y;
                        else if (startValue.b3) //percent
                            tweener.startValue.y = startValue.f2 * this._owner.height;
                        if (!endValue.b1)
                            tweener.endValue.x = tweener.startValue.x;
                        else if (endValue.b3)
                            tweener.endValue.x = endValue.f1 * this._owner.width;
                        if (!endValue.b2)
                            tweener.endValue.y = tweener.startValue.y;
                        else if (endValue.b3)
                            tweener.endValue.y = endValue.f2 * this._owner.height;
                    }
                    else {
                        if (!startValue.b1)
                            tweener.startValue.x = item.target.x - this._ownerBaseX;
                        if (!startValue.b2)
                            tweener.startValue.y = item.target.y - this._ownerBaseY;
                        if (!endValue.b1)
                            tweener.endValue.x = tweener.startValue.x;
                        if (!endValue.b2)
                            tweener.endValue.y = tweener.startValue.y;
                    }
                }
                else {
                    if (!startValue.b1)
                        tweener.startValue.x = item.target.width;
                    if (!startValue.b2)
                        tweener.startValue.y = item.target.height;
                    if (!endValue.b1)
                        tweener.endValue.x = tweener.startValue.x;
                    if (!endValue.b2)
                        tweener.endValue.y = tweener.startValue.y;
                }
                if (item.tweenConfig.path) {
                    item.value.b1 = item.value.b2 = true;
                    tweener.setPath(item.tweenConfig.path);
                }
            }
            this.callHook(item, false);
        }
        onTweenUpdate(tweener) {
            var item = tweener.target;
            switch (item.type) {
                case ActionType.XY:
                case ActionType.Size:
                case ActionType.Scale:
                case ActionType.Skew:
                    item.value.f1 = tweener.value.x;
                    item.value.f2 = tweener.value.y;
                    if (item.tweenConfig.path) {
                        item.value.f1 += tweener.startValue.x;
                        item.value.f2 += tweener.startValue.y;
                    }
                    break;
                case ActionType.Alpha:
                case ActionType.Rotation:
                    item.value.f1 = tweener.value.x;
                    break;
                case ActionType.Color:
                    item.value.f1 = tweener.value.color;
                    break;
                case ActionType.ColorFilter:
                    item.value.f1 = tweener.value.x;
                    item.value.f2 = tweener.value.y;
                    item.value.f3 = tweener.value.z;
                    item.value.f4 = tweener.value.w;
                    break;
                case ActionType.Shake:
                    item.value.offsetX = tweener.deltaValue.x;
                    item.value.offsetY = tweener.deltaValue.y;
                    break;
            }
            this.applyValue(item);
        }
        onTweenComplete(tweener) {
            var item = tweener.target;
            item.tweener = null;
            this._totalTasks--;
            if (tweener.allCompleted) //当整体播放结束时间在这个tween的中间时不应该调用结尾钩子
                this.callHook(item, true);
            this.checkAllComplete();
        }
        onPlayTransCompleted(item) {
            this._totalTasks--;
            this.checkAllComplete();
        }
        callHook(item, tweenEnd) {
            if (tweenEnd) {
                if (item.tweenConfig && item.tweenConfig.endHook)
                    typeof item.tweenConfig.endHook == "function" ? item.tweenConfig.endHook() : item.tweenConfig.endHook.run();
            }
            else {
                if (item.time >= this._startTime && item.hook) {
                    typeof item.hook == "function" ? item.hook() : item.hook.run();
                }
            }
        }
        checkAllComplete() {
            if (this._playing && this._totalTasks == 0) {
                if (this._totalTimes < 0) {
                    this.internalPlay();
                    if (this._totalTasks == 0)
                        fgui.GTween.delayedCall(0).setTarget(this).onComplete(this.checkAllComplete, this);
                }
                else {
                    this._totalTimes--;
                    if (this._totalTimes > 0) {
                        this.internalPlay();
                        if (this._totalTasks == 0)
                            fgui.GTween.delayedCall(0).setTarget(this).onComplete(this.checkAllComplete, this);
                    }
                    else {
                        this._playing = false;
                        var cnt = this._items.length;
                        for (var i = 0; i < cnt; i++) {
                            var item = this._items[i];
                            if (item.target && item.displayLockToken != 0) {
                                item.target.releaseDisplayLock(item.displayLockToken);
                                item.displayLockToken = 0;
                            }
                        }
                        if (this._onComplete) {
                            var handler = this._onComplete;
                            this._onComplete = null;
                            typeof handler === 'function' ? handler() : handler.run();
                        }
                    }
                }
            }
        }
        applyValue(item) {
            item.target._gearLocked = true;
            var value = item.value;
            switch (item.type) {
                case ActionType.XY:
                    if (item.target == this._owner) {
                        if (value.b1 && value.b2)
                            item.target.setXY(value.f1 + this._ownerBaseX, value.f2 + this._ownerBaseY);
                        else if (value.b1)
                            item.target.x = value.f1 + this._ownerBaseX;
                        else
                            item.target.y = value.f2 + this._ownerBaseY;
                    }
                    else {
                        if (value.b3) //position in percent
                         {
                            if (value.b1 && value.b2)
                                item.target.setXY(value.f1 * this._owner.width, value.f2 * this._owner.height);
                            else if (value.b1)
                                item.target.x = value.f1 * this._owner.width;
                            else if (value.b2)
                                item.target.y = value.f2 * this._owner.height;
                        }
                        else {
                            if (value.b1 && value.b2)
                                item.target.setXY(value.f1, value.f2);
                            else if (value.b1)
                                item.target.x = value.f1;
                            else if (value.b2)
                                item.target.y = value.f2;
                        }
                    }
                    break;
                case ActionType.Size:
                    if (!value.b1)
                        value.f1 = item.target.width;
                    if (!value.b2)
                        value.f2 = item.target.height;
                    item.target.setSize(value.f1, value.f2);
                    break;
                case ActionType.Pivot:
                    item.target.setPivot(value.f1, value.f2, item.target.pivotAsAnchor);
                    break;
                case ActionType.Alpha:
                    item.target.alpha = value.f1;
                    break;
                case ActionType.Rotation:
                    item.target.rotation = value.f1;
                    break;
                case ActionType.Scale:
                    item.target.setScale(value.f1, value.f2);
                    break;
                case ActionType.Skew:
                    item.target.setSkew(value.f1, value.f2);
                    break;
                case ActionType.Color:
                    item.target.setProp(fgui.ObjectPropID.Color, fgui.ToolSet.convertToHtmlColor(value.f1, false));
                    break;
                case ActionType.Animation:
                    if (value.frame >= 0)
                        item.target.setProp(fgui.ObjectPropID.Frame, value.frame);
                    item.target.setProp(fgui.ObjectPropID.Playing, value.playing);
                    item.target.setProp(fgui.ObjectPropID.TimeScale, this._timeScale);
                    break;
                case ActionType.Visible:
                    item.target.visible = value.visible;
                    break;
                case ActionType.Transition:
                    if (this._playing) {
                        var trans = value.trans;
                        if (trans) {
                            this._totalTasks++;
                            var startTime = this._startTime > item.time ? (this._startTime - item.time) : 0;
                            var endTime = this._endTime >= 0 ? (this._endTime - item.time) : -1;
                            if (value.stopTime >= 0 && (endTime < 0 || endTime > value.stopTime))
                                endTime = value.stopTime;
                            trans.timeScale = this._timeScale;
                            trans._play(() => this.onPlayTransCompleted(item), value.playTimes, 0, startTime, endTime, this._reversed);
                        }
                    }
                    break;
                case ActionType.Sound:
                    if (this._playing && item.time >= this._startTime) {
                        if (value.audioClip == null) {
                            var pi = fgui.UIPackage.getItemByURL(value.sound);
                            if (pi)
                                value.audioClip = pi.file;
                            else
                                value.audioClip = value.sound;
                        }
                        if (value.audioClip)
                            fgui.GRoot.inst.playOneShotSound(value.audioClip, value.volume);
                    }
                    break;
                case ActionType.Shake:
                    item.target.setXY(item.target.x - value.lastOffsetX + value.offsetX, item.target.y - value.lastOffsetY + value.offsetY);
                    value.lastOffsetX = value.offsetX;
                    value.lastOffsetY = value.offsetY;
                    break;
                case ActionType.ColorFilter:
                    {
                        fgui.ToolSet.setColorFilter(item.target.displayObject, [value.f1, value.f2, value.f3, value.f4]);
                        break;
                    }
                case ActionType.Text:
                    item.target.text = value.text;
                    break;
                case ActionType.Icon:
                    item.target.icon = value.text;
                    break;
            }
            item.target._gearLocked = false;
        }
        setup(buffer) {
            this.name = buffer.readS();
            this._options = buffer.getInt32();
            this._autoPlay = buffer.readBool();
            this._autoPlayTimes = buffer.getInt32();
            this._autoPlayDelay = buffer.getFloat32();
            var cnt = buffer.getInt16();
            for (var i = 0; i < cnt; i++) {
                var dataLen = buffer.getInt16();
                var curPos = buffer.pos;
                buffer.seek(curPos, 0);
                var item = new Item(buffer.readByte());
                this._items[i] = item;
                item.time = buffer.getFloat32();
                var targetId = buffer.getInt16();
                if (targetId < 0)
                    item.targetId = "";
                else
                    item.targetId = this._owner.getChildAt(targetId).id;
                item.label = buffer.readS();
                if (buffer.readBool()) {
                    buffer.seek(curPos, 1);
                    item.tweenConfig = new TweenConfig();
                    item.tweenConfig.duration = buffer.getFloat32();
                    if (item.time + item.tweenConfig.duration > this._totalDuration)
                        this._totalDuration = item.time + item.tweenConfig.duration;
                    item.tweenConfig.easeType = buffer.readByte();
                    item.tweenConfig.repeat = buffer.getInt32();
                    item.tweenConfig.yoyo = buffer.readBool();
                    item.tweenConfig.endLabel = buffer.readS();
                    buffer.seek(curPos, 2);
                    this.decodeValue(item, buffer, item.tweenConfig.startValue);
                    buffer.seek(curPos, 3);
                    this.decodeValue(item, buffer, item.tweenConfig.endValue);
                    if (buffer.version >= 2) {
                        var pathLen = buffer.getInt32();
                        if (pathLen > 0) {
                            item.tweenConfig.path = new fgui.GPath();
                            var pts = new Array();
                            for (var j = 0; j < pathLen; j++) {
                                var curveType = buffer.getUint8();
                                switch (curveType) {
                                    case fgui.CurveType.Bezier:
                                        pts.push(fgui.GPathPoint.newBezierPoint(buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32()));
                                        break;
                                    case fgui.CurveType.CubicBezier:
                                        pts.push(fgui.GPathPoint.newCubicBezierPoint(buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32(), buffer.getFloat32()));
                                        break;
                                    default:
                                        pts.push(fgui.GPathPoint.newPoint(buffer.getFloat32(), buffer.getFloat32(), curveType));
                                        break;
                                }
                            }
                            item.tweenConfig.path.create(pts);
                        }
                    }
                }
                else {
                    if (item.time > this._totalDuration)
                        this._totalDuration = item.time;
                    buffer.seek(curPos, 2);
                    this.decodeValue(item, buffer, item.value);
                }
                buffer.pos = curPos + dataLen;
            }
        }
        decodeValue(item, buffer, value) {
            switch (item.type) {
                case ActionType.XY:
                case ActionType.Size:
                case ActionType.Pivot:
                case ActionType.Skew:
                    value.b1 = buffer.readBool();
                    value.b2 = buffer.readBool();
                    value.f1 = buffer.getFloat32();
                    value.f2 = buffer.getFloat32();
                    if (buffer.version >= 2 && item.type == ActionType.XY)
                        value.b3 = buffer.readBool(); //percent
                    break;
                case ActionType.Alpha:
                case ActionType.Rotation:
                    value.f1 = buffer.getFloat32();
                    break;
                case ActionType.Scale:
                    value.f1 = buffer.getFloat32();
                    value.f2 = buffer.getFloat32();
                    break;
                case ActionType.Color:
                    value.f1 = buffer.readColor();
                    break;
                case ActionType.Animation:
                    value.playing = buffer.readBool();
                    value.frame = buffer.getInt32();
                    break;
                case ActionType.Visible:
                    value.visible = buffer.readBool();
                    break;
                case ActionType.Sound:
                    value.sound = buffer.readS();
                    value.volume = buffer.getFloat32();
                    break;
                case ActionType.Transition:
                    value.transName = buffer.readS();
                    value.playTimes = buffer.getInt32();
                    break;
                case ActionType.Shake:
                    value.amplitude = buffer.getFloat32();
                    value.duration = buffer.getFloat32();
                    break;
                case ActionType.ColorFilter:
                    value.f1 = buffer.getFloat32();
                    value.f2 = buffer.getFloat32();
                    value.f3 = buffer.getFloat32();
                    value.f4 = buffer.getFloat32();
                    break;
                case ActionType.Text:
                case ActionType.Icon:
                    value.text = buffer.readS();
                    break;
            }
        }
    }
    fgui.Transition = Transition;
    class ActionType {
    }
    ActionType.XY = 0;
    ActionType.Size = 1;
    ActionType.Scale = 2;
    ActionType.Pivot = 3;
    ActionType.Alpha = 4;
    ActionType.Rotation = 5;
    ActionType.Color = 6;
    ActionType.Animation = 7;
    ActionType.Visible = 8;
    ActionType.Sound = 9;
    ActionType.Transition = 10;
    ActionType.Shake = 11;
    ActionType.ColorFilter = 12;
    ActionType.Skew = 13;
    ActionType.Text = 14;
    ActionType.Icon = 15;
    ActionType.Unknown = 16;
    class Item {
        constructor(type) {
            this.type = type;
            this.value = {};
            this.displayLockToken = 0;
        }
    }
    class TweenConfig {
        constructor() {
            this.easeType = fgui.EaseType.QuadOut;
            this.startValue = { b1: true, b2: true };
            this.endValue = { b1: true, b2: true };
        }
    }
    const OPTION_IGNORE_DISPLAY_CONTROLLER = 1;
    const OPTION_AUTO_STOP_DISABLED = 2;
    const OPTION_AUTO_STOP_AT_END = 4;
})(fgui);

(function (fgui) {
    class TranslationHelper {
        constructor() {
        }
        static loadFromXML(source) {
            let strings = {};
            TranslationHelper.strings = strings;
            let xml = new Laya.XML(source);
            let resNode = xml.getNode("resources");
            let nodes = resNode.elements();
            let length1 = nodes.length;
            for (let i1 = 0; i1 < length1; i1++) {
                let cxml = nodes[i1];
                if (cxml.name == "string") {
                    let key = cxml.getAttrString("name");
                    let text = cxml.text;
                    let i = key.indexOf("-");
                    if (i == -1)
                        continue;
                    let key2 = key.substring(0, i);
                    let key3 = key.substring(i + 1);
                    let col = strings[key2];
                    if (!col) {
                        col = {};
                        strings[key2] = col;
                    }
                    col[key3] = text;
                }
            }
        }
        static translateComponent(item) {
            if (TranslationHelper.strings == null)
                return;
            let compStrings = TranslationHelper.strings[item.owner.id + item.id];
            if (compStrings == null)
                return;
            let elementId, value;
            let buffer = item.rawData;
            let nextPos;
            let itemCount;
            let dataLen;
            let curPos;
            let valueCnt;
            let page;
            buffer.seek(0, 2);
            let childCount = buffer.getInt16();
            for (let i = 0; i < childCount; i++) {
                dataLen = buffer.getInt16();
                curPos = buffer.pos;
                buffer.seek(curPos, 0);
                let baseType = buffer.readByte();
                let type = baseType;
                buffer.skip(4);
                elementId = buffer.readS();
                if (type == fgui.ObjectType.Component) {
                    if (buffer.seek(curPos, 6))
                        type = buffer.readByte();
                }
                buffer.seek(curPos, 1);
                if ((value = compStrings[elementId + "-tips"]) != null)
                    buffer.writeS(value);
                buffer.seek(curPos, 2);
                let gearCnt = buffer.getInt16();
                for (let j = 0; j < gearCnt; j++) {
                    nextPos = buffer.getInt16();
                    nextPos += buffer.pos;
                    if (buffer.readByte() == 6) //gearText
                     {
                        buffer.skip(2); //controller
                        valueCnt = buffer.getInt16();
                        for (let k = 0; k < valueCnt; k++) {
                            page = buffer.readS();
                            if (page != null) {
                                if ((value = compStrings[elementId + "-texts_" + k]) != null)
                                    buffer.writeS(value);
                                else
                                    buffer.skip(2);
                            }
                        }
                        if (buffer.readBool() && (value = compStrings[elementId + "-texts_def"]) != null)
                            buffer.writeS(value);
                    }
                    buffer.pos = nextPos;
                }
                if (baseType == fgui.ObjectType.Component && buffer.version >= 2) {
                    buffer.seek(curPos, 4);
                    buffer.skip(2); //pageController
                    buffer.skip(4 * buffer.getUint16());
                    let cpCount = buffer.getUint16();
                    for (let k = 0; k < cpCount; k++) {
                        let target = buffer.readS();
                        let propertyId = buffer.getUint16();
                        if (propertyId == 0 && (value = compStrings[elementId + "-cp-" + target]) != null)
                            buffer.writeS(value);
                        else
                            buffer.skip(2);
                    }
                }
                switch (type) {
                    case fgui.ObjectType.Text:
                    case fgui.ObjectType.RichText:
                    case fgui.ObjectType.InputText:
                        {
                            if ((value = compStrings[elementId]) != null) {
                                buffer.seek(curPos, 6);
                                buffer.writeS(value);
                            }
                            if ((value = compStrings[elementId + "-prompt"]) != null) {
                                buffer.seek(curPos, 4);
                                buffer.writeS(value);
                            }
                            break;
                        }
                    case fgui.ObjectType.List:
                    case fgui.ObjectType.Tree:
                        {
                            buffer.seek(curPos, 8);
                            buffer.skip(2);
                            itemCount = buffer.getUint16();
                            for (let j = 0; j < itemCount; j++) {
                                nextPos = buffer.getUint16();
                                nextPos += buffer.pos;
                                buffer.skip(2); //url
                                if (type == fgui.ObjectType.Tree)
                                    buffer.skip(2);
                                //title
                                if ((value = compStrings[elementId + "-" + j]) != null)
                                    buffer.writeS(value);
                                else
                                    buffer.skip(2);
                                //selected title
                                if ((value = compStrings[elementId + "-" + j + "-0"]) != null)
                                    buffer.writeS(value);
                                else
                                    buffer.skip(2);
                                if (buffer.version >= 2) {
                                    buffer.skip(6);
                                    buffer.skip(buffer.getUint16() * 4); //controllers
                                    let cpCount = buffer.getUint16();
                                    for (let k = 0; k < cpCount; k++) {
                                        let target = buffer.readS();
                                        let propertyId = buffer.getUint16();
                                        if (propertyId == 0 && (value = compStrings[elementId + "-" + j + "-" + target]) != null)
                                            buffer.writeS(value);
                                        else
                                            buffer.skip(2);
                                    }
                                }
                                buffer.pos = nextPos;
                            }
                            break;
                        }
                    case fgui.ObjectType.Label:
                        {
                            if (buffer.seek(curPos, 6) && buffer.readByte() == type) {
                                if ((value = compStrings[elementId]) != null)
                                    buffer.writeS(value);
                                else
                                    buffer.skip(2);
                                buffer.skip(2);
                                if (buffer.readBool())
                                    buffer.skip(4);
                                buffer.skip(4);
                                if (buffer.readBool() && (value = compStrings[elementId + "-prompt"]) != null)
                                    buffer.writeS(value);
                            }
                            break;
                        }
                    case fgui.ObjectType.Button:
                        {
                            if (buffer.seek(curPos, 6) && buffer.readByte() == type) {
                                if ((value = compStrings[elementId]) != null)
                                    buffer.writeS(value);
                                else
                                    buffer.skip(2);
                                if ((value = compStrings[elementId + "-0"]) != null)
                                    buffer.writeS(value);
                            }
                            break;
                        }
                    case fgui.ObjectType.ComboBox:
                        {
                            if (buffer.seek(curPos, 6) && buffer.readByte() == type) {
                                itemCount = buffer.getInt16();
                                for (let j = 0; j < itemCount; j++) {
                                    nextPos = buffer.getInt16();
                                    nextPos += buffer.pos;
                                    if ((value = compStrings[elementId + "-" + j]) != null)
                                        buffer.writeS(value);
                                    buffer.pos = nextPos;
                                }
                                if ((value = compStrings[elementId]) != null)
                                    buffer.writeS(value);
                            }
                            break;
                        }
                }
                buffer.pos = curPos + dataLen;
            }
        }
    }
    fgui.TranslationHelper = TranslationHelper;
})(fgui);

(function (fgui) {
    class UIConfig {
        constructor() {
        }
        //Default font name
        static get defaultFont() { return Laya.Config.defaultFont; }
        static set defaultFont(value) { Laya.Config.defaultFont = value; }
    }
    //When a modal window is in front, the background becomes dark.
    UIConfig.modalLayerColor = "rgba(33,33,33,0.2)";
    UIConfig.buttonSoundVolumeScale = 1;
    //Scrolling step in pixels
    UIConfig.defaultScrollStep = 25;
    //Deceleration ratio of scrollpane when its in touch dragging.
    UIConfig.defaultScrollDecelerationRate = 0.967;
    //Default scrollbar display mode. Recommened visible for Desktop and Auto for mobile.
    UIConfig.defaultScrollBarDisplay = fgui.ScrollBarDisplayType.Visible;
    //Allow dragging the content to scroll. Recommeded true for mobile.
    UIConfig.defaultScrollTouchEffect = true;
    //The "rebound" effect in the scolling container. Recommeded true for mobile.
    UIConfig.defaultScrollBounceEffect = true;
    /**
      * 当滚动容器设置为“贴近ITEM”时，判定贴近到哪一个ITEM的滚动距离阀值。
      */
    UIConfig.defaultScrollSnappingThreshold = 0.1;
    /**
      * 当滚动容器设置为“页面模式”时，判定翻到哪一页的滚动距离阀值。
      */
    UIConfig.defaultScrollPagingThreshold = 0.3;
    //Max items displayed in combobox without scrolling.
    UIConfig.defaultComboBoxVisibleItemCount = 10;
    // Pixel offsets of finger to trigger scrolling.
    UIConfig.touchScrollSensitivity = 20;
    // Pixel offsets of finger to trigger dragging.
    UIConfig.touchDragSensitivity = 10;
    // Pixel offsets of mouse pointer to trigger dragging.
    UIConfig.clickDragSensitivity = 2;
    // When click the window, brings to front automatically.
    UIConfig.bringWindowToFrontOnClick = true;
    UIConfig.frameTimeForAsyncUIConstruction = 2;
    UIConfig.textureLinearSampling = true;
    UIConfig.packageFileExtension = "fui";
    UIConfig.useLayaSkeleton = false;
    fgui.UIConfig = UIConfig;
})(fgui);

(function (fgui) {
    class UIObjectFactory {
        constructor() {
        }
        static setExtension(url, type) {
            if (url == null)
                throw "Invaild url: " + url;
            var pi = fgui.UIPackage.getItemByURL(url);
            if (pi)
                pi.extensionType = type;
            UIObjectFactory.extensions[url] = type;
        }
        static setPackageItemExtension(url, type) {
            UIObjectFactory.setExtension(url, type);
        }
        static setLoaderExtension(type) {
            UIObjectFactory.loaderType = type;
        }
        static resolvePackageItemExtension(pi) {
            var extensionType = UIObjectFactory.extensions["ui://" + pi.owner.id + pi.id];
            if (!extensionType)
                extensionType = UIObjectFactory.extensions["ui://" + pi.owner.name + "/" + pi.name];
            if (extensionType)
                pi.extensionType = extensionType;
        }
        static newObject(type, userClass) {
            var obj;
            if (typeof type === 'number') {
                switch (type) {
                    case fgui.ObjectType.Image:
                        return new fgui.GImage();
                    case fgui.ObjectType.MovieClip:
                        return new fgui.GMovieClip();
                    case fgui.ObjectType.Component:
                        return new fgui.GComponent();
                    case fgui.ObjectType.Text:
                        return new fgui.GTextField();
                    case fgui.ObjectType.RichText:
                        return new fgui.GRichTextField();
                    case fgui.ObjectType.InputText:
                        return new fgui.GTextInput();
                    case fgui.ObjectType.Group:
                        return new fgui.GGroup();
                    case fgui.ObjectType.List:
                        return new fgui.GList();
                    case fgui.ObjectType.Graph:
                        return new fgui.GGraph();
                    case fgui.ObjectType.Loader:
                        if (UIObjectFactory.loaderType)
                            return new UIObjectFactory.loaderType();
                        else
                            return new fgui.GLoader();
                    case fgui.ObjectType.Button:
                        return new fgui.GButton();
                    case fgui.ObjectType.Label:
                        return new fgui.GLabel();
                    case fgui.ObjectType.ProgressBar:
                        return new fgui.GProgressBar();
                    case fgui.ObjectType.Slider:
                        return new fgui.GSlider();
                    case fgui.ObjectType.ScrollBar:
                        return new fgui.GScrollBar();
                    case fgui.ObjectType.ComboBox:
                        return new fgui.GComboBox();
                    case fgui.ObjectType.Tree:
                        return new fgui.GTree();
                    case fgui.ObjectType.Loader3D:
                        return new fgui.GLoader3D();
                    default:
                        return null;
                }
            }
            else {
                if (type.type == fgui.PackageItemType.Component) {
                    if (userClass)
                        obj = new userClass();
                    else if (type.extensionType)
                        obj = new type.extensionType();
                    else
                        obj = UIObjectFactory.newObject(type.objectType);
                }
                else
                    obj = UIObjectFactory.newObject(type.objectType);
                if (obj)
                    obj.packageItem = type;
            }
            return obj;
        }
    }
    UIObjectFactory.extensions = {};
    fgui.UIObjectFactory = UIObjectFactory;
})(fgui);

(function (fgui) {
    class UIPackage {
        constructor() {
            this._items = [];
            this._itemsById = {};
            this._itemsByName = {};
            this._sprites = {};
            this._dependencies = [];
            this._branches = [];
            this._branchIndex = -1;
        }
        static get branch() {
            return UIPackage._branch;
        }
        static set branch(value) {
            UIPackage._branch = value;
            for (var pkgId in UIPackage._instById) {
                var pkg = UIPackage._instById[pkgId];
                if (pkg._branches) {
                    pkg._branchIndex = pkg._branches.indexOf(value);
                }
            }
        }
        static getVar(key) {
            return UIPackage._vars[key];
        }
        static setVar(key, value) {
            UIPackage._vars[key] = value;
        }
        static getById(id) {
            return UIPackage._instById[id];
        }
        static getByName(name) {
            return UIPackage._instByName[name];
        }
        static addPackage(resKey, descData) {
            if (!descData) {
                descData = fgui.AssetProxy.inst.getRes(resKey + "." + fgui.UIConfig.packageFileExtension);
                if (!descData || descData.byteLength == 0)
                    throw new Error("resource '" + resKey + "' not found");
            }
            var buffer = new fgui.ByteBuffer(descData);
            var pkg = new UIPackage();
            pkg._resKey = resKey;
            pkg.loadPackage(buffer);
            UIPackage._instById[pkg.id] = pkg;
            UIPackage._instByName[pkg.name] = pkg;
            UIPackage._instById[resKey] = pkg;
            return pkg;
        }
        /**
         * @param resKey resKey 或 [resKey1,resKey2,resKey3....]
         */
        static loadPackage(resKey, completeHandler, progressHandler) {
            let loadKeyArr = [];
            let keys = [];
            let i;
            if (Array.isArray(resKey)) {
                for (i = 0; i < resKey.length; i++) {
                    loadKeyArr.push({ url: resKey[i] + "." + fgui.UIConfig.packageFileExtension, type: Laya.Loader.BUFFER });
                    keys.push(resKey[i]);
                }
            }
            else {
                loadKeyArr = [{ url: resKey + "." + fgui.UIConfig.packageFileExtension, type: Laya.Loader.BUFFER }];
                keys = [resKey];
            }
            let pkgArr = [];
            let pkg;
            for (i = 0; i < loadKeyArr.length; i++) {
                pkg = UIPackage._instById[keys[i]];
                if (pkg) {
                    pkgArr.push(pkg);
                    loadKeyArr.splice(i, 1);
                    keys.splice(i, 1);
                    i--;
                }
            }
            if (loadKeyArr.length == 0 && completeHandler) {
                typeof completeHandler === 'function' ? completeHandler(pkgArr) : completeHandler.runWith([pkgArr]);
                return;
            }
            fgui.AssetProxy.inst.load(loadKeyArr, Laya.Loader.BUFFER).then((resArr) => {
                let pkg;
                let urls = [];
                for (i = 0; i < loadKeyArr.length; i++) {
                    let asset = resArr[i];
                    if (asset) {
                        pkg = new UIPackage();
                        pkgArr.push(pkg);
                        pkg._resKey = keys[i];
                        pkg.loadPackage(new fgui.ByteBuffer(asset.data));
                        let cnt = pkg._items.length;
                        for (let j = 0; j < cnt; j++) {
                            let pi = pkg._items[j];
                            if (pi.type == fgui.PackageItemType.Atlas) {
                                urls.push({ url: pi.file, type: Laya.Loader.IMAGE });
                            }
                            else if (pi.type == fgui.PackageItemType.Sound) {
                                urls.push({ url: pi.file, type: Laya.Loader.SOUND });
                            }
                        }
                    }
                }
                if (urls.length > 0) {
                    fgui.AssetProxy.inst.load(urls, null, (progress) => {
                        if (progressHandler)
                            typeof progressHandler === 'function' ? progressHandler(progress) : progressHandler.runWith(progress);
                    }).then(() => {
                        for (i = 0; i < pkgArr.length; i++) {
                            pkg = pkgArr[i];
                            if (!UIPackage._instById[pkg.id]) {
                                UIPackage._instById[pkg.id] = pkg;
                                UIPackage._instByName[pkg.name] = pkg;
                                UIPackage._instById[pkg._resKey] = pkg;
                            }
                        }
                        typeof completeHandler === 'function' ? completeHandler(pkgArr) : completeHandler.runWith([pkgArr]);
                    });
                }
                else {
                    for (i = 0; i < pkgArr.length; i++) {
                        pkg = pkgArr[i];
                        if (!UIPackage._instById[pkg.id]) {
                            UIPackage._instById[pkg.id] = pkg;
                            UIPackage._instByName[pkg.name] = pkg;
                            UIPackage._instById[pkg._resKey] = pkg;
                        }
                    }
                    typeof completeHandler === 'function' ? completeHandler(pkgArr) : completeHandler.runWith([pkgArr]);
                }
            });
        }
        static removePackage(packageIdOrName) {
            var pkg = UIPackage._instById[packageIdOrName];
            if (!pkg)
                pkg = UIPackage._instByName[packageIdOrName];
            if (!pkg)
                throw new Error("unknown package: " + packageIdOrName);
            pkg.dispose();
            delete UIPackage._instById[pkg.id];
            delete UIPackage._instByName[pkg.name];
            delete UIPackage._instById[pkg._resKey];
            if (pkg._customId)
                delete UIPackage._instById[pkg._customId];
        }
        static createObject(pkgName, resName, userClass) {
            var pkg = UIPackage.getByName(pkgName);
            if (pkg)
                return pkg.createObject(resName, userClass);
            else
                return null;
        }
        static createObjectFromURL(url, userClass) {
            var pi = UIPackage.getItemByURL(url);
            if (pi)
                return pi.owner.internalCreateObject(pi, userClass);
            else
                return null;
        }
        static getItemURL(pkgName, resName) {
            var pkg = UIPackage.getByName(pkgName);
            if (!pkg)
                return null;
            var pi = pkg._itemsByName[resName];
            if (!pi)
                return null;
            return "ui://" + pkg.id + pi.id;
        }
        static getItemByURL(url) {
            var pos1 = url.indexOf("//");
            if (pos1 == -1)
                return null;
            var pos2 = url.indexOf("/", pos1 + 2);
            if (pos2 == -1) {
                if (url.length > 13) {
                    var pkgId = url.substr(5, 8);
                    var pkg = UIPackage.getById(pkgId);
                    if (pkg) {
                        var srcId = url.substr(13);
                        return pkg.getItemById(srcId);
                    }
                }
            }
            else {
                var pkgName = url.substr(pos1 + 2, pos2 - pos1 - 2);
                pkg = UIPackage.getByName(pkgName);
                if (pkg) {
                    var srcName = url.substr(pos2 + 1);
                    return pkg.getItemByName(srcName);
                }
            }
            return null;
        }
        static getItemAssetByURL(url) {
            var item = UIPackage.getItemByURL(url);
            if (item == null)
                return null;
            return item.owner.getItemAsset(item);
        }
        static normalizeURL(url) {
            if (url == null)
                return null;
            var pos1 = url.indexOf("//");
            if (pos1 == -1)
                return null;
            var pos2 = url.indexOf("/", pos1 + 2);
            if (pos2 == -1)
                return url;
            var pkgName = url.substr(pos1 + 2, pos2 - pos1 - 2);
            var srcName = url.substr(pos2 + 1);
            return UIPackage.getItemURL(pkgName, srcName);
        }
        static setStringsSource(source) {
            fgui.TranslationHelper.loadFromXML(source);
        }
        loadPackage(buffer) {
            if (buffer.getUint32() != 0x46475549)
                throw new Error("FairyGUI: old package format found in '" + this._resKey + "'");
            buffer.version = buffer.getInt32();
            var compressed = buffer.readBool();
            this._id = buffer.readUTFString();
            this._name = buffer.readUTFString();
            buffer.skip(20);
            if (compressed) {
                var buf = new Uint8Array(buffer.buffer, buffer.pos, buffer.length - buffer.pos);
                var inflater = new Zlib.RawInflate(buf);
                buf = inflater.decompress();
                let buffer2 = new fgui.ByteBuffer(buf);
                buffer2.version = buffer.version;
                buffer = buffer2;
            }
            var ver2 = buffer.version >= 2;
            var indexTablePos = buffer.pos;
            var cnt;
            var i;
            var j;
            var nextPos;
            var str;
            var branchIncluded;
            buffer.seek(indexTablePos, 4);
            cnt = buffer.getInt32();
            var stringTable = [];
            for (i = 0; i < cnt; i++)
                stringTable[i] = buffer.readUTFString();
            buffer.stringTable = stringTable;
            buffer.seek(indexTablePos, 0);
            cnt = buffer.getInt16();
            for (i = 0; i < cnt; i++)
                this._dependencies.push({ id: buffer.readS(), name: buffer.readS() });
            if (ver2) {
                cnt = buffer.getInt16();
                if (cnt > 0) {
                    this._branches = buffer.readSArray(cnt);
                    if (UIPackage._branch)
                        this._branchIndex = this._branches.indexOf(UIPackage._branch);
                }
                branchIncluded = cnt > 0;
            }
            buffer.seek(indexTablePos, 1);
            var pi;
            var path = this._resKey;
            let pos = path.lastIndexOf('/');
            let shortPath = pos == -1 ? "" : path.substr(0, pos + 1);
            path = path + "_";
            cnt = buffer.getUint16();
            for (i = 0; i < cnt; i++) {
                nextPos = buffer.getInt32();
                nextPos += buffer.pos;
                pi = new fgui.PackageItem();
                pi.owner = this;
                pi.type = buffer.readByte();
                pi.id = buffer.readS();
                pi.name = buffer.readS();
                buffer.readS(); //path
                str = buffer.readS();
                if (str)
                    pi.file = str;
                buffer.readBool(); //exported
                pi.width = buffer.getInt32();
                pi.height = buffer.getInt32();
                switch (pi.type) {
                    case fgui.PackageItemType.Image:
                        {
                            pi.objectType = fgui.ObjectType.Image;
                            var scaleOption = buffer.readByte();
                            if (scaleOption == 1) {
                                pi.scale9Grid = new Laya.Rectangle();
                                pi.scale9Grid.x = buffer.getInt32();
                                pi.scale9Grid.y = buffer.getInt32();
                                pi.scale9Grid.width = buffer.getInt32();
                                pi.scale9Grid.height = buffer.getInt32();
                                pi.tileGridIndice = buffer.getInt32();
                            }
                            else if (scaleOption == 2)
                                pi.scaleByTile = true;
                            pi.smoothing = buffer.readBool();
                            break;
                        }
                    case fgui.PackageItemType.MovieClip:
                        {
                            pi.smoothing = buffer.readBool();
                            pi.objectType = fgui.ObjectType.MovieClip;
                            pi.rawData = buffer.readBuffer();
                            break;
                        }
                    case fgui.PackageItemType.Font:
                        {
                            pi.rawData = buffer.readBuffer();
                            break;
                        }
                    case fgui.PackageItemType.Component:
                        {
                            var extension = buffer.readByte();
                            if (extension > 0)
                                pi.objectType = extension;
                            else
                                pi.objectType = fgui.ObjectType.Component;
                            pi.rawData = buffer.readBuffer();
                            fgui.UIObjectFactory.resolvePackageItemExtension(pi);
                            break;
                        }
                    case fgui.PackageItemType.Atlas:
                    case fgui.PackageItemType.Sound:
                    case fgui.PackageItemType.Misc:
                        {
                            pi.file = path + pi.file;
                            break;
                        }
                    case fgui.PackageItemType.Spine:
                    case fgui.PackageItemType.DragonBones:
                        {
                            pi.file = shortPath + pi.file;
                            pi.skeletonAnchor = new Laya.Point();
                            pi.skeletonAnchor.x = buffer.getFloat32();
                            pi.skeletonAnchor.y = buffer.getFloat32();
                            break;
                        }
                }
                if (ver2) {
                    str = buffer.readS(); //branch
                    if (str)
                        pi.name = str + "/" + pi.name;
                    var branchCnt = buffer.getUint8();
                    if (branchCnt > 0) {
                        if (branchIncluded)
                            pi.branches = buffer.readSArray(branchCnt);
                        else
                            this._itemsById[buffer.readS()] = pi;
                    }
                    var highResCnt = buffer.getUint8();
                    if (highResCnt > 0)
                        pi.highResolution = buffer.readSArray(highResCnt);
                }
                this._items.push(pi);
                this._itemsById[pi.id] = pi;
                if (pi.name != null)
                    this._itemsByName[pi.name] = pi;
                buffer.pos = nextPos;
            }
            buffer.seek(indexTablePos, 2);
            cnt = buffer.getUint16();
            for (i = 0; i < cnt; i++) {
                nextPos = buffer.getUint16();
                nextPos += buffer.pos;
                var itemId = buffer.readS();
                pi = this._itemsById[buffer.readS()];
                let sprite = { atlas: pi, rect: new Laya.Rectangle(), offset: new Laya.Point(), originalSize: new Laya.Point() };
                sprite.atlas = pi;
                sprite.rect.x = buffer.getInt32();
                sprite.rect.y = buffer.getInt32();
                sprite.rect.width = buffer.getInt32();
                sprite.rect.height = buffer.getInt32();
                sprite.rotated = buffer.readBool();
                if (ver2 && buffer.readBool()) {
                    sprite.offset.x = buffer.getInt32();
                    sprite.offset.y = buffer.getInt32();
                    sprite.originalSize.x = buffer.getInt32();
                    sprite.originalSize.y = buffer.getInt32();
                }
                else {
                    sprite.originalSize.x = sprite.rect.width;
                    sprite.originalSize.y = sprite.rect.height;
                }
                this._sprites[itemId] = sprite;
                buffer.pos = nextPos;
            }
            if (buffer.seek(indexTablePos, 3)) {
                cnt = buffer.getUint16();
                for (i = 0; i < cnt; i++) {
                    nextPos = buffer.getInt32();
                    nextPos += buffer.pos;
                    pi = this._itemsById[buffer.readS()];
                    if (pi && pi.type == fgui.PackageItemType.Image) {
                        pi.pixelHitTestData = new fgui.PixelHitTestData();
                        pi.pixelHitTestData.load(buffer);
                    }
                    buffer.pos = nextPos;
                }
            }
        }
        loadAllAssets() {
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var pi = this._items[i];
                this.getItemAsset(pi);
            }
        }
        unloadAssets() {
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var pi = this._items[i];
                if (pi.type == fgui.PackageItemType.Atlas) {
                    if (pi.texture)
                        Laya.loader.clearTextureRes(pi.texture.url);
                }
            }
        }
        dispose() {
            var cnt = this._items.length;
            for (var i = 0; i < cnt; i++) {
                var pi = this._items[i];
                if (pi.type == fgui.PackageItemType.Atlas) {
                    if (pi.texture) {
                        pi.texture.destroy();
                        pi.texture = null;
                    }
                }
                else if (pi.type == fgui.PackageItemType.Sound) {
                    Laya.SoundManager.destroySound(pi.file);
                }
                else if (pi.templet)
                    pi.templet.destroy();
            }
        }
        get id() {
            return this._id;
        }
        get name() {
            return this._name;
        }
        get items() {
            return this._items;
        }
        get customId() {
            return this._customId;
        }
        set customId(value) {
            if (this._customId)
                delete UIPackage._instById[this._customId];
            this._customId = value;
            if (this._customId)
                UIPackage._instById[this._customId] = this;
        }
        createObject(resName, userClass) {
            var pi = this._itemsByName[resName];
            if (pi)
                return this.internalCreateObject(pi, userClass);
            else
                return null;
        }
        internalCreateObject(item, userClass) {
            var g = fgui.UIObjectFactory.newObject(item, userClass);
            if (g == null)
                return null;
            UIPackage._constructing++;
            g.constructFromResource();
            UIPackage._constructing--;
            return g;
        }
        getItems() {
            return this._items;
        }
        getItemById(itemId) {
            return this._itemsById[itemId];
        }
        getItemByName(resName) {
            return this._itemsByName[resName];
        }
        getItemAssetByName(resName) {
            var pi = this._itemsByName[resName];
            if (pi == null) {
                throw "Resource not found -" + resName;
            }
            return this.getItemAsset(pi);
        }
        getItemAsset(item) {
            switch (item.type) {
                case fgui.PackageItemType.Image:
                    if (!item.decoded) {
                        item.decoded = true;
                        var sprite = this._sprites[item.id];
                        if (sprite) {
                            var atlasTexture = (this.getItemAsset(sprite.atlas));
                            if (atlasTexture) {
                                item.texture = Laya.Texture.create(atlasTexture, sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height, sprite.offset.x, sprite.offset.y, sprite.originalSize.x, sprite.originalSize.y);
                            }
                            else {
                                item.texture = null;
                            }
                        }
                        else
                            item.texture = null;
                    }
                    return item.texture;
                case fgui.PackageItemType.Atlas:
                    if (!item.decoded) {
                        item.decoded = true;
                        item.texture = fgui.AssetProxy.inst.getItemRes(item);
                        //if(!fgui.UIConfig.textureLinearSampling)
                        //item.texture.isLinearSampling = false;
                    }
                    return item.texture;
                case fgui.PackageItemType.Font:
                    if (!item.decoded) {
                        item.decoded = true;
                        this.loadFont(item);
                    }
                    return item.bitmapFont;
                case fgui.PackageItemType.MovieClip:
                    if (!item.decoded) {
                        item.decoded = true;
                        this.loadMovieClip(item);
                    }
                    return item.frames;
                case fgui.PackageItemType.Component:
                    return item.rawData;
                case fgui.PackageItemType.Misc:
                    if (item.file)
                        return fgui.AssetProxy.inst.getItemRes(item);
                    else
                        return null;
                default:
                    return null;
            }
        }
        getItemAssetAsync(item, onComplete) {
            if (item.decoded) {
                onComplete(null, item);
                return;
            }
            if (item.loading) {
                item.loading.push(onComplete);
                return;
            }
            switch (item.type) {
                case fgui.PackageItemType.Spine:
                case fgui.PackageItemType.DragonBones:
                    item.loading = [onComplete];
                    let url;
                    if (fgui.UIConfig.useLayaSkeleton) {
                        let pos = item.file.lastIndexOf('.');
                        url = item.file.substring(0, pos + 1).replace("_ske", "") + "sk";
                    }
                    else
                        url = { url: item.file, type: Laya.Loader.SPINE };
                    Laya.loader.load(url).then(templet => {
                        let arr = item.loading;
                        delete item.loading;
                        item.templet = templet;
                        arr.forEach(e => e(item ? null : "load error", item));
                    });
                    break;
                default:
                    this.getItemAsset(item);
                    onComplete(null, item);
                    break;
            }
        }
        loadMovieClip(item) {
            var buffer = item.rawData;
            buffer.seek(0, 0);
            item.interval = buffer.getInt32();
            item.swing = buffer.readBool();
            item.repeatDelay = buffer.getInt32();
            buffer.seek(0, 1);
            var frameCount = buffer.getInt16();
            item.frames = [];
            var spriteId;
            var sprite;
            var fx;
            var fy;
            for (var i = 0; i < frameCount; i++) {
                var nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                fx = buffer.getInt32();
                fy = buffer.getInt32();
                buffer.getInt32(); //width
                buffer.getInt32(); //height
                let frame = { addDelay: buffer.getInt32() };
                spriteId = buffer.readS();
                if (spriteId != null && (sprite = this._sprites[spriteId]) != null) {
                    var atlasTexture = (this.getItemAsset(sprite.atlas));
                    frame.texture = Laya.Texture.create(atlasTexture, sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height, fx, fy, item.width, item.height);
                }
                item.frames[i] = frame;
                buffer.pos = nextPos;
            }
        }
        loadFont(item) {
            item = item.getBranch();
            let font = new Laya.BitmapFont();
            item.bitmapFont = font;
            Laya.Text.registerBitmapFont("ui://" + this.id + item.id, font);
            let buffer = item.rawData;
            buffer.seek(0, 0);
            let ttf = buffer.readBool();
            font.tint = buffer.readBool();
            font.autoScaleSize = buffer.readBool();
            buffer.readBool(); //has channel
            font.fontSize = Math.max(buffer.getInt32(), 1);
            let xadvance = buffer.getInt32();
            let lineHeight = buffer.getInt32();
            font.lineHeight = Math.max(lineHeight, font.fontSize);
            let mainTexture = null;
            let mainSprite = this._sprites[item.id];
            if (mainSprite)
                mainTexture = (this.getItemAsset(mainSprite.atlas));
            buffer.seek(0, 1);
            let dict = font.dict;
            var cnt = buffer.getInt32();
            for (let i = 0; i < cnt; i++) {
                let nextPos = buffer.getInt16();
                nextPos += buffer.pos;
                let ch = buffer.getUint16();
                let bg = {};
                dict[ch] = bg;
                let img = buffer.readS();
                let bx = buffer.getInt32();
                let by = buffer.getInt32();
                bg.x = buffer.getInt32();
                bg.y = buffer.getInt32();
                bg.width = buffer.getInt32();
                bg.height = buffer.getInt32();
                bg.advance = buffer.getInt32();
                buffer.readByte(); //channel
                if (ttf) {
                    bg.texture = Laya.Texture.create(mainTexture, bx + mainSprite.rect.x, by + mainSprite.rect.y, bg.width, bg.height);
                }
                else {
                    let charImg = this._itemsById[img];
                    if (charImg) {
                        charImg = charImg.getBranch();
                        bg.width = charImg.width;
                        bg.height = charImg.height;
                        charImg = charImg.getHighResolution();
                        this.getItemAsset(charImg);
                        bg.texture = charImg.texture;
                    }
                    if (bg.advance == 0) {
                        if (xadvance == 0)
                            bg.advance = bg.x + bg.width;
                        else
                            bg.advance = xadvance;
                    }
                }
                buffer.pos = nextPos;
            }
        }
    }
    UIPackage._constructing = 0;
    UIPackage._instById = {};
    UIPackage._instByName = {};
    UIPackage._branch = "";
    UIPackage._vars = {};
    fgui.UIPackage = UIPackage;
})(fgui);

(function (fgui) {
    class Window extends fgui.GComponent {
        constructor() {
            super();
            this._requestingCmd = 0;
            this._uiSources = [];
            this.bringToFontOnClick = fgui.UIConfig.bringWindowToFrontOnClick;
            this.displayObject.on(Laya.Event.DISPLAY, this, this.__onShown);
            this.displayObject.on(Laya.Event.UNDISPLAY, this, this.__onHidden);
            this.displayObject.on(Laya.Event.MOUSE_DOWN, this, this.__mouseDown);
        }
        addUISource(source) {
            this._uiSources.push(source);
        }
        set contentPane(val) {
            if (this._contentPane != val) {
                if (this._contentPane)
                    this.removeChild(this._contentPane);
                this._contentPane = val;
                if (this._contentPane) {
                    this.addChild(this._contentPane);
                    this.setSize(this._contentPane.width, this._contentPane.height);
                    this._contentPane.addRelation(this, fgui.RelationType.Size);
                    this._frame = (this._contentPane.getChild("frame"));
                    if (this._frame) {
                        this.closeButton = this._frame.getChild("closeButton");
                        this.dragArea = this._frame.getChild("dragArea");
                        this.contentArea = this._frame.getChild("contentArea");
                    }
                }
            }
        }
        get contentPane() {
            return this._contentPane;
        }
        get frame() {
            return this._frame;
        }
        get closeButton() {
            return this._closeButton;
        }
        set closeButton(value) {
            if (this._closeButton)
                this._closeButton.offClick(this, this.closeEventHandler);
            this._closeButton = value;
            if (this._closeButton)
                this._closeButton.onClick(this, this.closeEventHandler);
        }
        get dragArea() {
            return this._dragArea;
        }
        set dragArea(value) {
            if (this._dragArea != value) {
                if (this._dragArea) {
                    this._dragArea.draggable = false;
                    this._dragArea.off(fgui.Events.DRAG_START, this, this.__dragStart);
                }
                this._dragArea = value;
                if (this._dragArea) {
                    if (this._dragArea instanceof fgui.GGraph)
                        this._dragArea.asGraph.drawRect(0, null, null);
                    this._dragArea.draggable = true;
                    this._dragArea.on(fgui.Events.DRAG_START, this, this.__dragStart);
                }
            }
        }
        get contentArea() {
            return this._contentArea;
        }
        set contentArea(value) {
            this._contentArea = value;
        }
        show() {
            fgui.GRoot.inst.showWindow(this);
        }
        showOn(root) {
            root.showWindow(this);
        }
        hide() {
            if (this.isShowing)
                this.doHideAnimation();
        }
        hideImmediately() {
            var r = (this.parent instanceof fgui.GRoot) ? this.parent : null;
            if (!r)
                r = fgui.GRoot.inst;
            r.hideWindowImmediately(this);
        }
        centerOn(r, restraint) {
            this.setXY(Math.round((r.width - this.width) / 2), Math.round((r.height - this.height) / 2));
            if (restraint) {
                this.addRelation(r, fgui.RelationType.Center_Center);
                this.addRelation(r, fgui.RelationType.Middle_Middle);
            }
        }
        toggleStatus() {
            if (this.isTop)
                this.hide();
            else
                this.show();
        }
        get isShowing() {
            return this.parent != null;
        }
        get isTop() {
            return this.parent != null && this.parent.getChildIndex(this) == this.parent.numChildren - 1;
        }
        get modal() {
            return this._modal;
        }
        set modal(val) {
            this._modal = val;
        }
        bringToFront() {
            this.root.bringToFront(this);
        }
        showModalWait(requestingCmd) {
            if (requestingCmd != null)
                this._requestingCmd = requestingCmd;
            if (fgui.UIConfig.windowModalWaiting) {
                if (!this._modalWaitPane)
                    this._modalWaitPane = fgui.UIPackage.createObjectFromURL(fgui.UIConfig.windowModalWaiting);
                this.layoutModalWaitPane();
                this.addChild(this._modalWaitPane);
            }
        }
        layoutModalWaitPane() {
            if (this._contentArea) {
                var pt = this._frame.localToGlobal();
                pt = this.globalToLocal(pt.x, pt.y, pt);
                this._modalWaitPane.setXY(pt.x + this._contentArea.x, pt.y + this._contentArea.y);
                this._modalWaitPane.setSize(this._contentArea.width, this._contentArea.height);
            }
            else
                this._modalWaitPane.setSize(this.width, this.height);
        }
        closeModalWait(requestingCmd) {
            if (requestingCmd != null) {
                if (this._requestingCmd != requestingCmd)
                    return false;
            }
            this._requestingCmd = 0;
            if (this._modalWaitPane && this._modalWaitPane.parent != null)
                this.removeChild(this._modalWaitPane);
            return true;
        }
        get modalWaiting() {
            return this._modalWaitPane && this._modalWaitPane.parent != null;
        }
        init() {
            if (this._inited || this._loading)
                return;
            if (this._uiSources.length > 0) {
                this._loading = false;
                var cnt = this._uiSources.length;
                for (var i = 0; i < cnt; i++) {
                    var lib = this._uiSources[i];
                    if (!lib.loaded) {
                        lib.load(this.__uiLoadComplete, this);
                        this._loading = true;
                    }
                }
                if (!this._loading)
                    this._init();
            }
            else
                this._init();
        }
        onInit() {
        }
        onShown() {
        }
        onHide() {
        }
        doShowAnimation() {
            this.onShown();
        }
        doHideAnimation() {
            this.hideImmediately();
        }
        __uiLoadComplete() {
            var cnt = this._uiSources.length;
            for (var i = 0; i < cnt; i++) {
                var lib = this._uiSources[i];
                if (!lib.loaded)
                    return;
            }
            this._loading = false;
            this._init();
        }
        _init() {
            this._inited = true;
            this.onInit();
            if (this.isShowing)
                this.doShowAnimation();
        }
        dispose() {
            if (this.parent)
                this.hideImmediately();
            super.dispose();
        }
        closeEventHandler() {
            this.hide();
        }
        __onShown() {
            if (!this._inited)
                this.init();
            else
                this.doShowAnimation();
        }
        __onHidden() {
            this.closeModalWait();
            this.onHide();
        }
        __mouseDown() {
            if (this.isShowing && this.bringToFontOnClick)
                this.bringToFront();
        }
        __dragStart(evt) {
            fgui.GObject.cast(evt.currentTarget).stopDrag();
            this.startDrag();
        }
    }
    fgui.Window = Window;
})(fgui);

(function (fgui) {
    class ControllerAction {
        static createAction(type) {
            switch (type) {
                case 0:
                    return new fgui.PlayTransitionAction();
                case 1:
                    return new fgui.ChangePageAction();
            }
            return null;
        }
        constructor() {
        }
        run(controller, prevPage, curPage) {
            if ((this.fromPage == null || this.fromPage.length == 0 || this.fromPage.indexOf(prevPage) != -1)
                && (this.toPage == null || this.toPage.length == 0 || this.toPage.indexOf(curPage) != -1))
                this.enter(controller);
            else
                this.leave(controller);
        }
        enter(controller) {
        }
        leave(controller) {
        }
        setup(buffer) {
            var cnt;
            var i;
            cnt = buffer.getInt16();
            this.fromPage = [];
            for (i = 0; i < cnt; i++)
                this.fromPage[i] = buffer.readS();
            cnt = buffer.getInt16();
            this.toPage = [];
            for (i = 0; i < cnt; i++)
                this.toPage[i] = buffer.readS();
        }
    }
    fgui.ControllerAction = ControllerAction;
})(fgui);
///<reference path="ControllerAction.ts"/>

///<reference path="ControllerAction.ts"/>
(function (fgui) {
    class ChangePageAction extends fgui.ControllerAction {
        constructor() {
            super();
        }
        enter(controller) {
            if (!this.controllerName)
                return;
            var gcom;
            if (this.objectId)
                gcom = controller.parent.getChildById(this.objectId);
            else
                gcom = controller.parent;
            if (gcom) {
                var cc = gcom.getController(this.controllerName);
                if (cc && cc != controller && !cc.changing) {
                    if (this.targetPage == "~1") {
                        if (controller.selectedIndex < cc.pageCount)
                            cc.selectedIndex = controller.selectedIndex;
                    }
                    else if (this.targetPage == "~2")
                        cc.selectedPage = controller.selectedPage;
                    else
                        cc.selectedPageId = this.targetPage;
                }
            }
        }
        setup(buffer) {
            super.setup(buffer);
            this.objectId = buffer.readS();
            this.controllerName = buffer.readS();
            this.targetPage = buffer.readS();
        }
    }
    fgui.ChangePageAction = ChangePageAction;
})(fgui);

(function (fgui) {
    class PlayTransitionAction extends fgui.ControllerAction {
        constructor() {
            super();
        }
        enter(controller) {
            var trans = controller.parent.getTransition(this.transitionName);
            if (trans) {
                if (this._currentTransition && this._currentTransition.playing)
                    trans.changePlayTimes(this.playTimes);
                else
                    trans.play(null, this.playTimes, this.delay);
                this._currentTransition = trans;
            }
        }
        leave(controller) {
            if (this.stopOnExit && this._currentTransition) {
                this._currentTransition.stop();
                this._currentTransition = null;
            }
        }
        setup(buffer) {
            super.setup(buffer);
            this.transitionName = buffer.readS();
            this.playTimes = buffer.getInt32();
            this.delay = buffer.getFloat32();
            this.stopOnExit = buffer.readBool();
        }
    }
    fgui.PlayTransitionAction = PlayTransitionAction;
})(fgui);

(function (fgui) {
    function fillImage(w, h, method, origin, clockwise, amount) {
        if (amount <= 0)
            return null;
        else if (amount >= 0.9999)
            return [0, 0, w, 0, w, h, 0, h];
        var points;
        switch (method) {
            case fgui.FillMethod.Horizontal:
                points = fillHorizontal(w, h, origin, amount);
                break;
            case fgui.FillMethod.Vertical:
                points = fillVertical(w, h, origin, amount);
                break;
            case fgui.FillMethod.Radial90:
                points = fillRadial90(w, h, origin, clockwise, amount);
                break;
            case fgui.FillMethod.Radial180:
                points = fillRadial180(w, h, origin, clockwise, amount);
                break;
            case fgui.FillMethod.Radial360:
                points = fillRadial360(w, h, origin, clockwise, amount);
                break;
        }
        return points;
    }
    fgui.fillImage = fillImage;
    function fillHorizontal(w, h, origin, amount) {
        var w2 = w * amount;
        if (origin == fgui.FillOrigin.Left || origin == fgui.FillOrigin.Top)
            return [0, 0, w2, 0, w2, h, 0, h];
        else
            return [w, 0, w, h, w - w2, h, w - w2, 0];
    }
    function fillVertical(w, h, origin, amount) {
        var h2 = h * amount;
        if (origin == fgui.FillOrigin.Left || origin == fgui.FillOrigin.Top)
            return [0, 0, 0, h2, w, h2, w, 0];
        else
            return [0, h, w, h, w, h - h2, 0, h - h2];
    }
    function fillRadial90(w, h, origin, clockwise, amount) {
        if (clockwise && (origin == fgui.FillOrigin.TopRight || origin == fgui.FillOrigin.BottomLeft)
            || !clockwise && (origin == fgui.FillOrigin.TopLeft || origin == fgui.FillOrigin.BottomRight)) {
            amount = 1 - amount;
        }
        var v, v2, h2;
        v = Math.tan(Math.PI / 2 * amount);
        h2 = w * v;
        v2 = (h2 - h) / h2;
        var points;
        switch (origin) {
            case fgui.FillOrigin.TopLeft:
                if (clockwise) {
                    if (h2 <= h)
                        points = [0, 0, w, h2, w, 0];
                    else
                        points = [0, 0, w * (1 - v2), h, w, h, w, 0];
                }
                else {
                    if (h2 <= h)
                        points = [0, 0, w, h2, w, h, 0, h];
                    else
                        points = [0, 0, w * (1 - v2), h, 0, h];
                }
                break;
            case fgui.FillOrigin.TopRight:
                if (clockwise) {
                    if (h2 <= h)
                        points = [w, 0, 0, h2, 0, h, w, h];
                    else
                        points = [w, 0, w * v2, h, w, h];
                }
                else {
                    if (h2 <= h)
                        points = [w, 0, 0, h2, 0, 0];
                    else
                        points = [w, 0, w * v2, h, 0, h, 0, 0];
                }
                break;
            case fgui.FillOrigin.BottomLeft:
                if (clockwise) {
                    if (h2 <= h)
                        points = [0, h, w, h - h2, w, 0, 0, 0];
                    else
                        points = [0, h, w * (1 - v2), 0, 0, 0];
                }
                else {
                    if (h2 <= h)
                        points = [0, h, w, h - h2, w, h];
                    else
                        points = [0, h, w * (1 - v2), 0, w, 0, w, h];
                }
                break;
            case fgui.FillOrigin.BottomRight:
                if (clockwise) {
                    if (h2 <= h)
                        points = [w, h, 0, h - h2, 0, h];
                    else
                        points = [w, h, w * v2, 0, 0, 0, 0, h];
                }
                else {
                    if (h2 <= h)
                        points = [w, h, 0, h - h2, 0, 0, w, 0];
                    else
                        points = [w, h, w * v2, 0, w, 0];
                }
                break;
        }
        return points;
    }
    function movePoints(points, offsetX, offsetY) {
        var cnt = points.length;
        for (var i = 0; i < cnt; i += 2) {
            points[i] += offsetX;
            points[i + 1] += offsetY;
        }
    }
    function fillRadial180(w, h, origin, clockwise, amount) {
        var points;
        switch (origin) {
            case fgui.FillOrigin.Top:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial90(w / 2, h, clockwise ? fgui.FillOrigin.TopLeft : fgui.FillOrigin.TopRight, clockwise, amount);
                    if (clockwise)
                        movePoints(points, w / 2, 0);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial90(w / 2, h, clockwise ? fgui.FillOrigin.TopRight : fgui.FillOrigin.TopLeft, clockwise, amount);
                    if (clockwise)
                        points.push(w, h, w, 0);
                    else {
                        movePoints(points, w / 2, 0);
                        points.push(0, h, 0, 0);
                    }
                }
                break;
            case fgui.FillOrigin.Bottom:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial90(w / 2, h, clockwise ? fgui.FillOrigin.BottomRight : fgui.FillOrigin.BottomLeft, clockwise, amount);
                    if (!clockwise)
                        movePoints(points, w / 2, 0);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial90(w / 2, h, clockwise ? fgui.FillOrigin.BottomLeft : fgui.FillOrigin.BottomRight, clockwise, amount);
                    if (clockwise) {
                        movePoints(points, w / 2, 0);
                        points.push(0, 0, 0, h);
                    }
                    else
                        points.push(w, 0, w, h);
                }
                break;
            case fgui.FillOrigin.Left:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial90(w, h / 2, clockwise ? fgui.FillOrigin.BottomLeft : fgui.FillOrigin.TopLeft, clockwise, amount);
                    if (!clockwise)
                        movePoints(points, 0, h / 2);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial90(w, h / 2, clockwise ? fgui.FillOrigin.TopLeft : fgui.FillOrigin.BottomLeft, clockwise, amount);
                    if (clockwise) {
                        movePoints(points, 0, h / 2);
                        points.push(w, 0, 0, 0);
                    }
                    else
                        points.push(w, h, 0, h);
                }
                break;
            case fgui.FillOrigin.Right:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial90(w, h / 2, clockwise ? fgui.FillOrigin.TopRight : fgui.FillOrigin.BottomRight, clockwise, amount);
                    if (clockwise)
                        movePoints(points, 0, h / 2);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial90(w, h / 2, clockwise ? fgui.FillOrigin.BottomRight : fgui.FillOrigin.TopRight, clockwise, amount);
                    if (clockwise)
                        points.push(0, h, w, h);
                    else {
                        movePoints(points, 0, h / 2);
                        points.push(0, 0, w, 0);
                    }
                }
                break;
        }
        return points;
    }
    function fillRadial360(w, h, origin, clockwise, amount) {
        var points;
        switch (origin) {
            case fgui.FillOrigin.Top:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial180(w / 2, h, clockwise ? fgui.FillOrigin.Left : fgui.FillOrigin.Right, clockwise, amount);
                    if (clockwise)
                        movePoints(points, w / 2, 0);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial180(w / 2, h, clockwise ? fgui.FillOrigin.Right : fgui.FillOrigin.Left, clockwise, amount);
                    if (clockwise)
                        points.push(w, h, w, 0, w / 2, 0);
                    else {
                        movePoints(points, w / 2, 0);
                        points.push(0, h, 0, 0, w / 2, 0);
                    }
                }
                break;
            case fgui.FillOrigin.Bottom:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial180(w / 2, h, clockwise ? fgui.FillOrigin.Right : fgui.FillOrigin.Left, clockwise, amount);
                    if (!clockwise)
                        movePoints(points, w / 2, 0);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial180(w / 2, h, clockwise ? fgui.FillOrigin.Left : fgui.FillOrigin.Right, clockwise, amount);
                    if (clockwise) {
                        movePoints(points, w / 2, 0);
                        points.push(0, 0, 0, h, w / 2, h);
                    }
                    else
                        points.push(w, 0, w, h, w / 2, h);
                }
                break;
            case fgui.FillOrigin.Left:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial180(w, h / 2, clockwise ? fgui.FillOrigin.Bottom : fgui.FillOrigin.Top, clockwise, amount);
                    if (!clockwise)
                        movePoints(points, 0, h / 2);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial180(w, h / 2, clockwise ? fgui.FillOrigin.Top : fgui.FillOrigin.Bottom, clockwise, amount);
                    if (clockwise) {
                        movePoints(points, 0, h / 2);
                        points.push(w, 0, 0, 0, 0, h / 2);
                    }
                    else
                        points.push(w, h, 0, h, 0, h / 2);
                }
                break;
            case fgui.FillOrigin.Right:
                if (amount <= 0.5) {
                    amount = amount / 0.5;
                    points = fillRadial180(w, h / 2, clockwise ? fgui.FillOrigin.Top : fgui.FillOrigin.Bottom, clockwise, amount);
                    if (clockwise)
                        movePoints(points, 0, h / 2);
                }
                else {
                    amount = (amount - 0.5) / 0.5;
                    points = fillRadial180(w, h / 2, clockwise ? fgui.FillOrigin.Bottom : fgui.FillOrigin.Top, clockwise, amount);
                    if (clockwise)
                        points.push(0, h, w, h, w, h / 2);
                    else {
                        movePoints(points, 0, h / 2);
                        points.push(0, 0, w, 0, w, h / 2);
                    }
                }
                break;
        }
        return points;
    }
})(fgui);

(function (fgui) {
    class Image extends Laya.Sprite {
        constructor() {
            super();
            this._tileGridIndice = 0;
            this._needRebuild = 0;
            this._fillMethod = 0;
            this._fillOrigin = 0;
            this._fillAmount = 0;
            this.mouseEnabled = false;
            this._color = "#FFFFFF";
        }
        set_width(value) {
            super.set_width(value);
            this.markChanged(1);
        }
        set_height(value) {
            super.set_height(value);
            this.markChanged(1);
        }
        get texture() {
            return this._source;
        }
        set texture(value) {
            if (this._source != value) {
                this._source = value;
                if (!this._isWidthSet && !this._isHeightSet) {
                    if (this._source)
                        this.size(this._source.width, this._source.height);
                    else
                        this.size(0, 0);
                }
                this.repaint();
                this.markChanged(1);
            }
        }
        get scale9Grid() {
            return this._scale9Grid;
        }
        set scale9Grid(value) {
            this._scale9Grid = value;
            this._sizeGrid = null;
            this.markChanged(1);
        }
        get scaleByTile() {
            return this._scaleByTile;
        }
        set scaleByTile(value) {
            if (this._scaleByTile != value) {
                this._scaleByTile = value;
                this.markChanged(1);
            }
        }
        get tileGridIndice() {
            return this._tileGridIndice;
        }
        set tileGridIndice(value) {
            if (this._tileGridIndice != value) {
                this._tileGridIndice = value;
                this.markChanged(1);
            }
        }
        get fillMethod() {
            return this._fillMethod;
        }
        set fillMethod(value) {
            if (this._fillMethod != value) {
                this._fillMethod = value;
                if (this._fillMethod != 0) {
                    if (!this._mask) {
                        this._mask = new Laya.Sprite();
                        this._mask.mouseEnabled = false;
                    }
                    this.mask = this._mask;
                    this.markChanged(2);
                }
                else if (this.mask) {
                    this._mask.graphics.clear();
                    this.mask = null;
                }
            }
        }
        get fillOrigin() {
            return this._fillOrigin;
        }
        set fillOrigin(value) {
            if (this._fillOrigin != value) {
                this._fillOrigin = value;
                if (this._fillMethod != 0)
                    this.markChanged(2);
            }
        }
        get fillClockwise() {
            return this._fillClockwise;
        }
        set fillClockwise(value) {
            if (this._fillClockwise != value) {
                this._fillClockwise = value;
                if (this._fillMethod != 0)
                    this.markChanged(2);
            }
        }
        get fillAmount() {
            return this._fillAmount;
        }
        set fillAmount(value) {
            if (this._fillAmount != value) {
                this._fillAmount = value;
                if (this._fillMethod != 0)
                    this.markChanged(2);
            }
        }
        get color() {
            return this._color;
        }
        set color(value) {
            if (this._color != value) {
                this._color = value;
                this.markChanged(1);
            }
        }
        markChanged(flag) {
            if (!this._needRebuild) {
                this._needRebuild = flag;
                Laya.timer.callLater(this, this.rebuild);
            }
            else
                this._needRebuild |= flag;
        }
        rebuild() {
            if ((this._needRebuild & 1) != 0)
                this.doDraw();
            if ((this._needRebuild & 2) != 0 && this._fillMethod != 0)
                this.doFill();
            this._needRebuild = 0;
        }
        doDraw() {
            var w = this.width;
            var h = this.height;
            var g = this.graphics;
            var tex = this._source;
            g.clear();
            if (tex == null || w == 0 || h == 0) {
                return;
            }
            if (this._scaleByTile) {
                g.fillTexture(tex, 0, 0, w, h);
            }
            else if (this._scale9Grid) {
                if (!this._sizeGrid) {
                    var tw = tex.width;
                    var th = tex.height;
                    var left = this._scale9Grid.x;
                    var right = Math.max(tw - this._scale9Grid.right, 0);
                    var top = this._scale9Grid.y;
                    var bottom = Math.max(th - this._scale9Grid.bottom, 0);
                    this._sizeGrid = [top, right, bottom, left, this._tileGridIndice];
                }
                g.draw9Grid(tex, 0, 0, w, h, this._sizeGrid, this._color);
            }
            else {
                g.drawImage(tex, 0, 0, w, h, this._color);
            }
        }
        doFill() {
            var w = this.width;
            var h = this.height;
            var g = this._mask.graphics;
            g.clear();
            if (w == 0 || h == 0)
                return;
            var points = fgui.fillImage(w, h, this._fillMethod, this._fillOrigin, this._fillClockwise, this._fillAmount);
            if (points == null) {
                //this.mask = null;
                //this.mask = this._mask;
                return;
            }
            g.drawPoly(0, 0, points, "#FFFFFF", null, 0);
        }
    }
    fgui.Image = Image;
})(fgui);
///<reference path="./Image.ts"/>

///<reference path="./Image.ts"/>
(function (fgui) {
    class MovieClip extends fgui.Image {
        constructor() {
            super();
            this.interval = 0;
            this.repeatDelay = 0;
            this.timeScale = 1;
            this._playing = true;
            this._frameCount = 0;
            this._frame = 0;
            this._start = 0;
            this._end = 0;
            this._times = 0;
            this._endAt = 0;
            this._status = 0; //0-none, 1-next loop, 2-ending, 3-ended
            this._frameElapsed = 0; //当前帧延迟
            this._repeatedCount = 0;
            this.mouseEnabled = false;
            this.setPlaySettings();
            this.on(Laya.Event.DISPLAY, this, this.__addToStage);
            this.on(Laya.Event.UNDISPLAY, this, this.__removeFromStage);
        }
        get frames() {
            return this._frames;
        }
        set frames(value) {
            this._frames = value;
            this._scaleByTile = false;
            this._scale9Grid = null;
            if (this._frames) {
                this._frameCount = this._frames.length;
                if (this._end == -1 || this._end > this._frameCount - 1)
                    this._end = this._frameCount - 1;
                if (this._endAt == -1 || this._endAt > this._frameCount - 1)
                    this._endAt = this._frameCount - 1;
                if (this._frame < 0 || this._frame > this._frameCount - 1)
                    this._frame = this._frameCount - 1;
                this._frameElapsed = 0;
                this._repeatedCount = 0;
                this._reversed = false;
            }
            else
                this._frameCount = 0;
            this.drawFrame();
            this.checkTimer();
        }
        get frameCount() {
            return this._frameCount;
        }
        get frame() {
            return this._frame;
        }
        set frame(value) {
            if (this._frame != value) {
                if (this._frames && value >= this._frameCount)
                    value = this._frameCount - 1;
                this._frame = value;
                this._frameElapsed = 0;
                this.drawFrame();
            }
        }
        get playing() {
            return this._playing;
        }
        set playing(value) {
            if (this._playing != value) {
                this._playing = value;
                this.checkTimer();
            }
        }
        //从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
        rewind() {
            this._frame = 0;
            this._frameElapsed = 0;
            this._reversed = false;
            this._repeatedCount = 0;
            this.drawFrame();
        }
        syncStatus(anotherMc) {
            this._frame = anotherMc._frame;
            this._frameElapsed = anotherMc._frameElapsed;
            this._reversed = anotherMc._reversed;
            this._repeatedCount = anotherMc._repeatedCount;
            this.drawFrame();
        }
        advance(timeInMiniseconds) {
            var beginFrame = this._frame;
            var beginReversed = this._reversed;
            var backupTime = timeInMiniseconds;
            while (true) {
                var tt = this.interval + this._frames[this._frame].addDelay;
                if (this._frame == 0 && this._repeatedCount > 0)
                    tt += this.repeatDelay;
                if (timeInMiniseconds < tt) {
                    this._frameElapsed = 0;
                    break;
                }
                timeInMiniseconds -= tt;
                if (this.swing) {
                    if (this._reversed) {
                        this._frame--;
                        if (this._frame <= 0) {
                            this._frame = 0;
                            this._repeatedCount++;
                            this._reversed = !this._reversed;
                        }
                    }
                    else {
                        this._frame++;
                        if (this._frame > this._frameCount - 1) {
                            this._frame = Math.max(0, this._frameCount - 2);
                            this._repeatedCount++;
                            this._reversed = !this._reversed;
                        }
                    }
                }
                else {
                    this._frame++;
                    if (this._frame > this._frameCount - 1) {
                        this._frame = 0;
                        this._repeatedCount++;
                    }
                }
                if (this._frame == beginFrame && this._reversed == beginReversed) //走了一轮了
                 {
                    var roundTime = backupTime - timeInMiniseconds; //这就是一轮需要的时间
                    timeInMiniseconds -= Math.floor(timeInMiniseconds / roundTime) * roundTime; //跳过
                }
            }
            this.drawFrame();
        }
        //从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
        setPlaySettings(start, end, times, endAt, endHandler) {
            if (start == undefined)
                start = 0;
            if (end == undefined)
                end = -1;
            if (times == undefined)
                times = 0;
            if (endAt == undefined)
                endAt = -1;
            this._start = start;
            this._end = end;
            if (this._end == -1 || this._end > this._frameCount - 1)
                this._end = this._frameCount - 1;
            this._times = times;
            this._endAt = endAt;
            if (this._endAt == -1)
                this._endAt = this._end;
            this._status = 0;
            this._endHandler = endHandler;
            this.frame = start;
        }
        update() {
            if (!this._playing || this._frameCount == 0 || this._status == 3)
                return;
            var dt = Laya.timer.delta;
            if (dt > 100)
                dt = 100;
            if (this.timeScale != 1)
                dt *= this.timeScale;
            this._frameElapsed += dt;
            var tt = this.interval + this._frames[this._frame].addDelay;
            if (this._frame == 0 && this._repeatedCount > 0)
                tt += this.repeatDelay;
            if (this._frameElapsed < tt)
                return;
            this._frameElapsed -= tt;
            if (this._frameElapsed > this.interval)
                this._frameElapsed = this.interval;
            if (this.swing) {
                if (this._reversed) {
                    this._frame--;
                    if (this._frame <= 0) {
                        this._frame = 0;
                        this._repeatedCount++;
                        this._reversed = !this._reversed;
                    }
                }
                else {
                    this._frame++;
                    if (this._frame > this._frameCount - 1) {
                        this._frame = Math.max(0, this._frameCount - 2);
                        this._repeatedCount++;
                        this._reversed = !this._reversed;
                    }
                }
            }
            else {
                this._frame++;
                if (this._frame > this._frameCount - 1) {
                    this._frame = 0;
                    this._repeatedCount++;
                }
            }
            if (this._status == 1) //new loop
             {
                this._frame = this._start;
                this._frameElapsed = 0;
                this._status = 0;
            }
            else if (this._status == 2) //ending
             {
                this._frame = this._endAt;
                this._frameElapsed = 0;
                this._status = 3; //ended
                //play end
                if (this._endHandler) {
                    var handler = this._endHandler;
                    this._endHandler = null;
                    if (typeof handler === 'function')
                        handler();
                    else
                        handler.run();
                }
            }
            else {
                if (this._frame == this._end) {
                    if (this._times > 0) {
                        this._times--;
                        if (this._times == 0)
                            this._status = 2; //ending
                        else
                            this._status = 1; //new loop
                    }
                    else {
                        this._status = 1; //new loop
                    }
                }
            }
            this.drawFrame();
        }
        drawFrame() {
            if (this._frameCount > 0 && this._frame < this._frames.length) {
                var frame = this._frames[this._frame];
                this.texture = frame.texture;
            }
            else
                this.texture = null;
            this.rebuild();
        }
        checkTimer() {
            if (this._playing && this._frameCount > 0 && this.stage != null)
                Laya.timer.frameLoop(1, this, this.update);
            else
                Laya.timer.clear(this, this.update);
        }
        __addToStage() {
            if (this._playing && this._frameCount > 0)
                Laya.timer.frameLoop(1, this, this.update);
        }
        __removeFromStage() {
            Laya.timer.clear(this, this.update);
        }
    }
    fgui.MovieClip = MovieClip;
})(fgui);

(function (fgui) {
    class GearBase {
        static create(owner, index) {
            if (!Classes)
                Classes = [
                    fgui.GearDisplay, fgui.GearXY, fgui.GearSize, fgui.GearLook, fgui.GearColor,
                    fgui.GearAnimation, fgui.GearText, fgui.GearIcon, fgui.GearDisplay2, fgui.GearFontSize
                ];
            return new (Classes[index])(owner);
        }
        constructor(owner) {
            this._owner = owner;
        }
        dispose() {
            if (this._tweenConfig && this._tweenConfig._tweener) {
                this._tweenConfig._tweener.kill();
                this._tweenConfig._tweener = null;
            }
        }
        get controller() {
            return this._controller;
        }
        set controller(val) {
            if (val != this._controller) {
                this._controller = val;
                if (this._controller)
                    this.init();
            }
        }
        get tweenConfig() {
            if (!this._tweenConfig)
                this._tweenConfig = new GearTweenConfig();
            return this._tweenConfig;
        }
        setup(buffer) {
            this._controller = this._owner.parent.getControllerAt(buffer.getInt16());
            this.init();
            var i;
            var page;
            var cnt = buffer.getInt16();
            if (this instanceof fgui.GearDisplay) {
                this.pages = buffer.readSArray(cnt);
            }
            else if (this instanceof fgui.GearDisplay2) {
                this.pages = buffer.readSArray(cnt);
            }
            else {
                for (i = 0; i < cnt; i++) {
                    page = buffer.readS();
                    if (page == null)
                        continue;
                    this.addStatus(page, buffer);
                }
                if (buffer.readBool())
                    this.addStatus(null, buffer);
            }
            if (buffer.readBool()) {
                this._tweenConfig = new GearTweenConfig();
                this._tweenConfig.easeType = buffer.readByte();
                this._tweenConfig.duration = buffer.getFloat32();
                this._tweenConfig.delay = buffer.getFloat32();
            }
            if (buffer.version >= 2) {
                if (this instanceof fgui.GearXY) {
                    if (buffer.readBool()) {
                        this.positionsInPercent = true;
                        for (i = 0; i < cnt; i++) {
                            page = buffer.readS();
                            if (page == null)
                                continue;
                            this.addExtStatus(page, buffer);
                        }
                        if (buffer.readBool())
                            this.addExtStatus(null, buffer);
                    }
                }
                else if (this instanceof fgui.GearDisplay2)
                    this.condition = buffer.readByte();
            }
        }
        updateFromRelations(dx, dy) {
        }
        addStatus(pageId, buffer) {
        }
        init() {
        }
        apply() {
        }
        updateState() {
        }
    }
    fgui.GearBase = GearBase;
    var Classes;
    class GearTweenConfig {
        constructor() {
            this.tween = true;
            this.easeType = fgui.EaseType.QuadOut;
            this.duration = 0.3;
            this.delay = 0;
        }
    }
    fgui.GearTweenConfig = GearTweenConfig;
})(fgui);
///<reference path="GearBase.ts"/>

///<reference path="GearBase.ts"/>
(function (fgui) {
    class GearAnimation extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = {
                playing: this._owner.getProp(fgui.ObjectPropID.Playing),
                frame: this._owner.getProp(fgui.ObjectPropID.Frame)
            };
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            var gv;
            if (pageId == null)
                gv = this._default;
            else
                this._storage[pageId] = gv = {};
            gv.playing = buffer.readBool();
            gv.frame = buffer.getInt32();
        }
        apply() {
            this._owner._gearLocked = true;
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                gv = this._default;
            this._owner.setProp(fgui.ObjectPropID.Playing, gv.playing);
            this._owner.setProp(fgui.ObjectPropID.Frame, gv.frame);
            this._owner._gearLocked = false;
        }
        updateState() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                this._storage[this._controller.selectedPageId] = gv = {};
            gv.playing = this._owner.getProp(fgui.ObjectPropID.Playing);
            gv.frame = this._owner.getProp(fgui.ObjectPropID.Frame);
        }
    }
    fgui.GearAnimation = GearAnimation;
})(fgui);

(function (fgui) {
    class GearColor extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = {
                color: this._owner.getProp(fgui.ObjectPropID.Color),
                strokeColor: this._owner.getProp(fgui.ObjectPropID.OutlineColor)
            };
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            var gv;
            if (pageId == null)
                gv = this._default;
            else
                this._storage[pageId] = gv = {};
            gv.color = buffer.readColorS();
            gv.strokeColor = buffer.readColorS();
        }
        apply() {
            this._owner._gearLocked = true;
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                gv = this._default;
            this._owner.setProp(fgui.ObjectPropID.Color, gv.color);
            this._owner.setProp(fgui.ObjectPropID.OutlineColor, gv.strokeColor);
            this._owner._gearLocked = false;
        }
        updateState() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                this._storage[this._controller.selectedPageId] = gv = {};
            gv.color = this._owner.getProp(fgui.ObjectPropID.Color);
            gv.strokeColor = this._owner.getProp(fgui.ObjectPropID.OutlineColor);
        }
    }
    fgui.GearColor = GearColor;
})(fgui);

(function (fgui) {
    class GearDisplay extends fgui.GearBase {
        constructor(owner) {
            super(owner);
            this._displayLockToken = 1;
            this._visible = 0;
        }
        init() {
            this.pages = null;
        }
        apply() {
            this._displayLockToken++;
            if (this._displayLockToken == 0)
                this._displayLockToken = 1;
            if (this.pages == null || this.pages.length == 0
                || this.pages.indexOf(this._controller.selectedPageId) != -1)
                this._visible = 1;
            else
                this._visible = 0;
        }
        addLock() {
            this._visible++;
            return this._displayLockToken;
        }
        releaseLock(token) {
            if (token == this._displayLockToken)
                this._visible--;
        }
        get connected() {
            return this._controller == null || this._visible > 0;
        }
    }
    fgui.GearDisplay = GearDisplay;
})(fgui);

(function (fgui) {
    class GearDisplay2 extends fgui.GearBase {
        constructor(owner) {
            super(owner);
            this._visible = 0;
        }
        init() {
            this.pages = null;
        }
        apply() {
            if (this.pages == null || this.pages.length == 0
                || this.pages.indexOf(this._controller.selectedPageId) != -1)
                this._visible = 1;
            else
                this._visible = 0;
        }
        evaluate(connected) {
            var v = this._controller == null || this._visible > 0;
            if (this.condition == 0)
                v = v && connected;
            else
                v = v || connected;
            return v;
        }
    }
    fgui.GearDisplay2 = GearDisplay2;
})(fgui);

(function (fgui) {
    class GearFontSize extends fgui.GearBase {
        constructor(owner) {
            super(owner);
            this._default = 0;
        }
        init() {
            this._default = this._owner.getProp(fgui.ObjectPropID.FontSize);
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            if (pageId == null)
                this._default = buffer.getInt32();
            else
                this._storage[pageId] = buffer.getInt32();
        }
        apply() {
            this._owner._gearLocked = true;
            var data = this._storage[this._controller.selectedPageId];
            if (data != undefined)
                this._owner.setProp(fgui.ObjectPropID.FontSize, data);
            else
                this._owner.setProp(fgui.ObjectPropID.FontSize, this._default);
            this._owner._gearLocked = false;
        }
        updateState() {
            this._storage[this._controller.selectedPageId] = this._owner.getProp(fgui.ObjectPropID.FontSize);
        }
    }
    fgui.GearFontSize = GearFontSize;
})(fgui);

(function (fgui) {
    class GearIcon extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = this._owner.icon;
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            if (pageId == null)
                this._default = buffer.readS();
            else
                this._storage[pageId] = buffer.readS();
        }
        apply() {
            this._owner._gearLocked = true;
            var data = this._storage[this._controller.selectedPageId];
            if (data !== undefined)
                this._owner.icon = data;
            else
                this._owner.icon = this._default;
            this._owner._gearLocked = false;
        }
        updateState() {
            this._storage[this._controller.selectedPageId] = this._owner.icon;
        }
    }
    fgui.GearIcon = GearIcon;
})(fgui);

(function (fgui) {
    class GearLook extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = {
                alpha: this._owner.alpha,
                rotation: this._owner.rotation,
                grayed: this._owner.grayed,
                touchable: this._owner.touchable
            };
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            var gv;
            if (pageId == null)
                gv = this._default;
            else
                this._storage[pageId] = gv = {};
            gv.alpha = buffer.getFloat32();
            gv.rotation = buffer.getFloat32();
            gv.grayed = buffer.readBool();
            gv.touchable = buffer.readBool();
        }
        apply() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                gv = this._default;
            if (this._tweenConfig && this._tweenConfig.tween && !fgui.UIPackage._constructing && !fgui.GearBase.disableAllTweenEffect) {
                this._owner._gearLocked = true;
                this._owner.grayed = gv.grayed;
                this._owner.touchable = gv.touchable;
                this._owner._gearLocked = false;
                if (this._tweenConfig._tweener) {
                    if (this._tweenConfig._tweener.endValue.x != gv.alpha || this._tweenConfig._tweener.endValue.y != gv.rotation) {
                        this._tweenConfig._tweener.kill(true);
                        this._tweenConfig._tweener = null;
                    }
                    else
                        return;
                }
                var a = gv.alpha != this._owner.alpha;
                var b = gv.rotation != this._owner.rotation;
                if (a || b) {
                    if (this._owner.checkGearController(0, this._controller))
                        this._tweenConfig._displayLockToken = this._owner.addDisplayLock();
                    this._tweenConfig._tweener = fgui.GTween.to2(this._owner.alpha, this._owner.rotation, gv.alpha, gv.rotation, this._tweenConfig.duration)
                        .setDelay(this._tweenConfig.delay)
                        .setEase(this._tweenConfig.easeType)
                        .setUserData((a ? 1 : 0) + (b ? 2 : 0))
                        .setTarget(this)
                        .onUpdate(this.__tweenUpdate, this)
                        .onComplete(this.__tweenComplete, this);
                }
            }
            else {
                this._owner._gearLocked = true;
                this._owner.grayed = gv.grayed;
                this._owner.touchable = gv.touchable;
                this._owner.alpha = gv.alpha;
                this._owner.rotation = gv.rotation;
                this._owner._gearLocked = false;
            }
        }
        __tweenUpdate(tweener) {
            var flag = tweener.userData;
            this._owner._gearLocked = true;
            if ((flag & 1) != 0)
                this._owner.alpha = tweener.value.x;
            if ((flag & 2) != 0)
                this._owner.rotation = tweener.value.y;
            this._owner._gearLocked = false;
        }
        __tweenComplete() {
            if (this._tweenConfig._displayLockToken != 0) {
                this._owner.releaseDisplayLock(this._tweenConfig._displayLockToken);
                this._tweenConfig._displayLockToken = 0;
            }
            this._tweenConfig._tweener = null;
        }
        updateState() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                this._storage[this._controller.selectedPageId] = gv = {};
            gv.alpha = this._owner.alpha;
            gv.rotation = this._owner.rotation;
            gv.grayed = this._owner.grayed;
            gv.touchable = this._owner.touchable;
        }
    }
    fgui.GearLook = GearLook;
})(fgui);

(function (fgui) {
    class GearSize extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = {
                width: this._owner.width,
                height: this._owner.height,
                scaleX: this._owner.scaleX,
                scaleY: this._owner.scaleY
            };
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            var gv;
            if (pageId == null)
                gv = this._default;
            else
                this._storage[pageId] = gv = {};
            gv.width = buffer.getInt32();
            gv.height = buffer.getInt32();
            gv.scaleX = buffer.getFloat32();
            gv.scaleY = buffer.getFloat32();
        }
        apply() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                gv = this._default;
            if (this._tweenConfig && this._tweenConfig.tween && !fgui.UIPackage._constructing && !fgui.GearBase.disableAllTweenEffect) {
                if (this._tweenConfig._tweener) {
                    if (this._tweenConfig._tweener.endValue.x != gv.width || this._tweenConfig._tweener.endValue.y != gv.height
                        || this._tweenConfig._tweener.endValue.z != gv.scaleX || this._tweenConfig._tweener.endValue.w != gv.scaleY) {
                        this._tweenConfig._tweener.kill(true);
                        this._tweenConfig._tweener = null;
                    }
                    else
                        return;
                }
                var a = gv.width != this._owner.width || gv.height != this._owner.height;
                var b = gv.scaleX != this._owner.scaleX || gv.scaleY != this._owner.scaleY;
                if (a || b) {
                    if (this._owner.checkGearController(0, this._controller))
                        this._tweenConfig._displayLockToken = this._owner.addDisplayLock();
                    this._tweenConfig._tweener = fgui.GTween.to4(this._owner.width, this._owner.height, this._owner.scaleX, this._owner.scaleY, gv.width, gv.height, gv.scaleX, gv.scaleY, this._tweenConfig.duration)
                        .setDelay(this._tweenConfig.delay)
                        .setEase(this._tweenConfig.easeType)
                        .setUserData((a ? 1 : 0) + (b ? 2 : 0))
                        .setTarget(this)
                        .onUpdate(this.__tweenUpdate, this)
                        .onComplete(this.__tweenComplete, this);
                }
            }
            else {
                this._owner._gearLocked = true;
                this._owner.setSize(gv.width, gv.height, this._owner.getGear(1).controller == this._controller);
                this._owner.setScale(gv.scaleX, gv.scaleY);
                this._owner._gearLocked = false;
            }
        }
        __tweenUpdate(tweener) {
            var flag = tweener.userData;
            this._owner._gearLocked = true;
            if ((flag & 1) != 0)
                this._owner.setSize(tweener.value.x, tweener.value.y, this._owner.checkGearController(1, this._controller));
            if ((flag & 2) != 0)
                this._owner.setScale(tweener.value.z, tweener.value.w);
            this._owner._gearLocked = false;
        }
        __tweenComplete() {
            if (this._tweenConfig._displayLockToken != 0) {
                this._owner.releaseDisplayLock(this._tweenConfig._displayLockToken);
                this._tweenConfig._displayLockToken = 0;
            }
            this._tweenConfig._tweener = null;
        }
        updateState() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                this._storage[this._controller.selectedPageId] = gv = {};
            gv.width = this._owner.width;
            gv.height = this._owner.height;
            gv.scaleX = this._owner.scaleX;
            gv.scaleY = this._owner.scaleY;
        }
        updateFromRelations(dx, dy) {
            if (this._controller == null || this._storage == null)
                return;
            for (var key in this._storage) {
                var gv = this._storage[key];
                gv.width += dx;
                gv.height += dy;
            }
            this._default.width += dx;
            this._default.height += dy;
            this.updateState();
        }
    }
    fgui.GearSize = GearSize;
})(fgui);

(function (fgui) {
    class GearText extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = this._owner.text;
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            if (pageId == null)
                this._default = buffer.readS();
            else
                this._storage[pageId] = buffer.readS();
        }
        apply() {
            this._owner._gearLocked = true;
            var data = this._storage[this._controller.selectedPageId];
            if (data !== undefined)
                this._owner.text = data;
            else
                this._owner.text = this._default;
            this._owner._gearLocked = false;
        }
        updateState() {
            this._storage[this._controller.selectedPageId] = this._owner.text;
        }
    }
    fgui.GearText = GearText;
})(fgui);

(function (fgui) {
    class GearXY extends fgui.GearBase {
        constructor(owner) {
            super(owner);
        }
        init() {
            this._default = {
                x: this._owner.x,
                y: this._owner.y,
                px: this._owner.x / this._owner.parent.width,
                py: this._owner.y / this._owner.parent.height
            };
            this._storage = {};
        }
        addStatus(pageId, buffer) {
            var gv;
            if (pageId == null)
                gv = this._default;
            else
                this._storage[pageId] = gv = {};
            gv.x = buffer.getInt32();
            gv.y = buffer.getInt32();
        }
        addExtStatus(pageId, buffer) {
            var gv;
            if (pageId == null)
                gv = this._default;
            else
                gv = this._storage[pageId];
            gv.px = buffer.getFloat32();
            gv.py = buffer.getFloat32();
        }
        apply() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                gv = this._default;
            var ex;
            var ey;
            if (this.positionsInPercent && this._owner.parent) {
                ex = gv.px * this._owner.parent.width;
                ey = gv.py * this._owner.parent.height;
            }
            else {
                ex = gv.x;
                ey = gv.y;
            }
            if (this._tweenConfig && this._tweenConfig.tween && !fgui.UIPackage._constructing && !fgui.GearBase.disableAllTweenEffect) {
                if (this._tweenConfig._tweener) {
                    if (this._tweenConfig._tweener.endValue.x != ex || this._tweenConfig._tweener.endValue.y != ey) {
                        this._tweenConfig._tweener.kill(true);
                        this._tweenConfig._tweener = null;
                    }
                    else
                        return;
                }
                var ox = this._owner.x;
                var oy = this._owner.y;
                if (ox != ex || oy != ey) {
                    if (this._owner.checkGearController(0, this._controller))
                        this._tweenConfig._displayLockToken = this._owner.addDisplayLock();
                    this._tweenConfig._tweener = fgui.GTween.to2(ox, oy, ex, ey, this._tweenConfig.duration)
                        .setDelay(this._tweenConfig.delay)
                        .setEase(this._tweenConfig.easeType)
                        .setTarget(this)
                        .onUpdate(this.__tweenUpdate, this)
                        .onComplete(this.__tweenComplete, this);
                }
            }
            else {
                this._owner._gearLocked = true;
                this._owner.setXY(ex, ey);
                this._owner._gearLocked = false;
            }
        }
        __tweenUpdate(tweener) {
            this._owner._gearLocked = true;
            this._owner.setXY(tweener.value.x, tweener.value.y);
            this._owner._gearLocked = false;
        }
        __tweenComplete() {
            if (this._tweenConfig._displayLockToken != 0) {
                this._owner.releaseDisplayLock(this._tweenConfig._displayLockToken);
                this._tweenConfig._displayLockToken = 0;
            }
            this._tweenConfig._tweener = null;
        }
        updateState() {
            var gv = this._storage[this._controller.selectedPageId];
            if (!gv)
                this._storage[this._controller.selectedPageId] = gv = {};
            gv.x = this._owner.x;
            gv.y = this._owner.y;
            gv.px = this._owner.x / this._owner.parent.width;
            gv.py = this._owner.y / this._owner.parent.height;
        }
        updateFromRelations(dx, dy) {
            if (this._controller == null || this._storage == null || this.positionsInPercent)
                return;
            for (var key in this._storage) {
                var pt = this._storage[key];
                pt.x += dx;
                pt.y += dy;
            }
            this._default.x += dx;
            this._default.y += dy;
            this.updateState();
        }
    }
    fgui.GearXY = GearXY;
})(fgui);
// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/07/19 14:11
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php
// 
// =============================================================
// Contains Daniele Giardini's C# port of the easing equations created by Robert Penner
// (all easing equations except for Flash, InFlash, OutFlash, InOutFlash,
// which use some parts of Robert Penner's equations but were created by Daniele Giardini)
// http://robertpenner.com/easing, see license below:
// =============================================================
//
// TERMS OF USE - EASING EQUATIONS
//
// Open source under the BSD License.
//
// Copyright ? 2001 Robert Penner
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// - Neither the name of the author nor the names of contributors may be used to endorse
// or promote products derived from this software without specific prior written permission.
// - THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/07/19 14:11
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php
// 
// =============================================================
// Contains Daniele Giardini's C# port of the easing equations created by Robert Penner
// (all easing equations except for Flash, InFlash, OutFlash, InOutFlash,
// which use some parts of Robert Penner's equations but were created by Daniele Giardini)
// http://robertpenner.com/easing, see license below:
// =============================================================
//
// TERMS OF USE - EASING EQUATIONS
//
// Open source under the BSD License.
//
// Copyright ? 2001 Robert Penner
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// - Neither the name of the author nor the names of contributors may be used to endorse
// or promote products derived from this software without specific prior written permission.
// - THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
(function (fgui) {
    const _PiOver2 = Math.PI * 0.5;
    const _TwoPi = Math.PI * 2;
    function evaluateEase(easeType, time, duration, overshootOrAmplitude, period) {
        switch (easeType) {
            case fgui.EaseType.Linear:
                return time / duration;
            case fgui.EaseType.SineIn:
                return -Math.cos(time / duration * _PiOver2) + 1;
            case fgui.EaseType.SineOut:
                return Math.sin(time / duration * _PiOver2);
            case fgui.EaseType.SineInOut:
                return -0.5 * (Math.cos(Math.PI * time / duration) - 1);
            case fgui.EaseType.QuadIn:
                return (time /= duration) * time;
            case fgui.EaseType.QuadOut:
                return -(time /= duration) * (time - 2);
            case fgui.EaseType.QuadInOut:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time;
                return -0.5 * ((--time) * (time - 2) - 1);
            case fgui.EaseType.CubicIn:
                return (time /= duration) * time * time;
            case fgui.EaseType.CubicOut:
                return ((time = time / duration - 1) * time * time + 1);
            case fgui.EaseType.CubicInOut:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time * time;
                return 0.5 * ((time -= 2) * time * time + 2);
            case fgui.EaseType.QuartIn:
                return (time /= duration) * time * time * time;
            case fgui.EaseType.QuartOut:
                return -((time = time / duration - 1) * time * time * time - 1);
            case fgui.EaseType.QuartInOut:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time * time * time;
                return -0.5 * ((time -= 2) * time * time * time - 2);
            case fgui.EaseType.QuintIn:
                return (time /= duration) * time * time * time * time;
            case fgui.EaseType.QuintOut:
                return ((time = time / duration - 1) * time * time * time * time + 1);
            case fgui.EaseType.QuintInOut:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * time * time * time * time * time;
                return 0.5 * ((time -= 2) * time * time * time * time + 2);
            case fgui.EaseType.ExpoIn:
                return (time == 0) ? 0 : Math.pow(2, 10 * (time / duration - 1));
            case fgui.EaseType.ExpoOut:
                if (time == duration)
                    return 1;
                return (-Math.pow(2, -10 * time / duration) + 1);
            case fgui.EaseType.ExpoInOut:
                if (time == 0)
                    return 0;
                if (time == duration)
                    return 1;
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * Math.pow(2, 10 * (time - 1));
                return 0.5 * (-Math.pow(2, -10 * --time) + 2);
            case fgui.EaseType.CircIn:
                return -(Math.sqrt(1 - (time /= duration) * time) - 1);
            case fgui.EaseType.CircOut:
                return Math.sqrt(1 - (time = time / duration - 1) * time);
            case fgui.EaseType.CircInOut:
                if ((time /= duration * 0.5) < 1)
                    return -0.5 * (Math.sqrt(1 - time * time) - 1);
                return 0.5 * (Math.sqrt(1 - (time -= 2) * time) + 1);
            case fgui.EaseType.ElasticIn:
                var s0;
                if (time == 0)
                    return 0;
                if ((time /= duration) == 1)
                    return 1;
                if (period == 0)
                    period = duration * 0.3;
                if (overshootOrAmplitude < 1) {
                    overshootOrAmplitude = 1;
                    s0 = period / 4;
                }
                else
                    s0 = period / _TwoPi * Math.asin(1 / overshootOrAmplitude);
                return -(overshootOrAmplitude * Math.pow(2, 10 * (time -= 1)) * Math.sin((time * duration - s0) * _TwoPi / period));
            case fgui.EaseType.ElasticOut:
                var s1;
                if (time == 0)
                    return 0;
                if ((time /= duration) == 1)
                    return 1;
                if (period == 0)
                    period = duration * 0.3;
                if (overshootOrAmplitude < 1) {
                    overshootOrAmplitude = 1;
                    s1 = period / 4;
                }
                else
                    s1 = period / _TwoPi * Math.asin(1 / overshootOrAmplitude);
                return (overshootOrAmplitude * Math.pow(2, -10 * time) * Math.sin((time * duration - s1) * _TwoPi / period) + 1);
            case fgui.EaseType.ElasticInOut:
                var s;
                if (time == 0)
                    return 0;
                if ((time /= duration * 0.5) == 2)
                    return 1;
                if (period == 0)
                    period = duration * (0.3 * 1.5);
                if (overshootOrAmplitude < 1) {
                    overshootOrAmplitude = 1;
                    s = period / 4;
                }
                else
                    s = period / _TwoPi * Math.asin(1 / overshootOrAmplitude);
                if (time < 1)
                    return -0.5 * (overshootOrAmplitude * Math.pow(2, 10 * (time -= 1)) * Math.sin((time * duration - s) * _TwoPi / period));
                return overshootOrAmplitude * Math.pow(2, -10 * (time -= 1)) * Math.sin((time * duration - s) * _TwoPi / period) * 0.5 + 1;
            case fgui.EaseType.BackIn:
                return (time /= duration) * time * ((overshootOrAmplitude + 1) * time - overshootOrAmplitude);
            case fgui.EaseType.BackOut:
                return ((time = time / duration - 1) * time * ((overshootOrAmplitude + 1) * time + overshootOrAmplitude) + 1);
            case fgui.EaseType.BackInOut:
                if ((time /= duration * 0.5) < 1)
                    return 0.5 * (time * time * (((overshootOrAmplitude *= (1.525)) + 1) * time - overshootOrAmplitude));
                return 0.5 * ((time -= 2) * time * (((overshootOrAmplitude *= (1.525)) + 1) * time + overshootOrAmplitude) + 2);
            case fgui.EaseType.BounceIn:
                return bounce_easeIn(time, duration);
            case fgui.EaseType.BounceOut:
                return bounce_easeOut(time, duration);
            case fgui.EaseType.BounceInOut:
                return bounce_easeInOut(time, duration);
            default:
                return -(time /= duration) * (time - 2);
        }
    }
    fgui.evaluateEase = evaluateEase;
    function bounce_easeIn(time, duration) {
        return 1 - bounce_easeOut(duration - time, duration);
    }
    function bounce_easeOut(time, duration) {
        if ((time /= duration) < (1 / 2.75)) {
            return (7.5625 * time * time);
        }
        if (time < (2 / 2.75)) {
            return (7.5625 * (time -= (1.5 / 2.75)) * time + 0.75);
        }
        if (time < (2.5 / 2.75)) {
            return (7.5625 * (time -= (2.25 / 2.75)) * time + 0.9375);
        }
        return (7.5625 * (time -= (2.625 / 2.75)) * time + 0.984375);
    }
    function bounce_easeInOut(time, duration) {
        if (time < duration * 0.5) {
            return bounce_easeIn(time * 2, duration) * 0.5;
        }
        return bounce_easeOut(time * 2 - duration, duration) * 0.5 + 0.5;
    }
})(fgui);

(function (fgui) {
    class EaseType {
    }
    EaseType.Linear = 0;
    EaseType.SineIn = 1;
    EaseType.SineOut = 2;
    EaseType.SineInOut = 3;
    EaseType.QuadIn = 4;
    EaseType.QuadOut = 5;
    EaseType.QuadInOut = 6;
    EaseType.CubicIn = 7;
    EaseType.CubicOut = 8;
    EaseType.CubicInOut = 9;
    EaseType.QuartIn = 10;
    EaseType.QuartOut = 11;
    EaseType.QuartInOut = 12;
    EaseType.QuintIn = 13;
    EaseType.QuintOut = 14;
    EaseType.QuintInOut = 15;
    EaseType.ExpoIn = 16;
    EaseType.ExpoOut = 17;
    EaseType.ExpoInOut = 18;
    EaseType.CircIn = 19;
    EaseType.CircOut = 20;
    EaseType.CircInOut = 21;
    EaseType.ElasticIn = 22;
    EaseType.ElasticOut = 23;
    EaseType.ElasticInOut = 24;
    EaseType.BackIn = 25;
    EaseType.BackOut = 26;
    EaseType.BackInOut = 27;
    EaseType.BounceIn = 28;
    EaseType.BounceOut = 29;
    EaseType.BounceInOut = 30;
    EaseType.Custom = 31;
    fgui.EaseType = EaseType;
})(fgui);

(function (fgui) {
    class GPath {
        constructor() {
            this._segments = new Array();
            this._points = new Array();
        }
        get length() {
            return this._fullLength;
        }
        create(pt1, pt2, pt3, pt4) {
            var points;
            if (Array.isArray(pt1))
                points = pt1;
            else {
                points = new Array();
                points.push(pt1);
                points.push(pt2);
                if (pt3)
                    points.push(pt3);
                if (pt4)
                    points.push(pt4);
            }
            this._segments.length = 0;
            this._points.length = 0;
            this._fullLength = 0;
            var cnt = points.length;
            if (cnt == 0)
                return;
            var splinePoints = s_points;
            splinePoints.length = 0;
            var prev = points[0];
            if (prev.curveType == fgui.CurveType.CRSpline)
                splinePoints.push(new Laya.Point(prev.x, prev.y));
            for (var i = 1; i < cnt; i++) {
                var current = points[i];
                if (prev.curveType != fgui.CurveType.CRSpline) {
                    var seg = {};
                    seg.type = prev.curveType;
                    seg.ptStart = this._points.length;
                    if (prev.curveType == fgui.CurveType.Straight) {
                        seg.ptCount = 2;
                        this._points.push(new Laya.Point(prev.x, prev.y));
                        this._points.push(new Laya.Point(current.x, current.y));
                    }
                    else if (prev.curveType == fgui.CurveType.Bezier) {
                        seg.ptCount = 3;
                        this._points.push(new Laya.Point(prev.x, prev.y));
                        this._points.push(new Laya.Point(current.x, current.y));
                        this._points.push(new Laya.Point(prev.control1_x, prev.control1_y));
                    }
                    else if (prev.curveType == fgui.CurveType.CubicBezier) {
                        seg.ptCount = 4;
                        this._points.push(new Laya.Point(prev.x, prev.y));
                        this._points.push(new Laya.Point(current.x, current.y));
                        this._points.push(new Laya.Point(prev.control1_x, prev.control1_y));
                        this._points.push(new Laya.Point(prev.control2_x, prev.control2_y));
                    }
                    seg.length = fgui.ToolSet.distance(prev.x, prev.y, current.x, current.y);
                    this._fullLength += seg.length;
                    this._segments.push(seg);
                }
                if (current.curveType != fgui.CurveType.CRSpline) {
                    if (splinePoints.length > 0) {
                        splinePoints.push(new Laya.Point(current.x, current.y));
                        this.createSplineSegment();
                    }
                }
                else
                    splinePoints.push(new Laya.Point(current.x, current.y));
                prev = current;
            }
            if (splinePoints.length > 1)
                this.createSplineSegment();
        }
        createSplineSegment() {
            var splinePoints = s_points;
            var cnt = splinePoints.length;
            splinePoints.splice(0, 0, splinePoints[0]);
            splinePoints.push(splinePoints[cnt]);
            splinePoints.push(splinePoints[cnt]);
            cnt += 3;
            var seg = {};
            seg.type = fgui.CurveType.CRSpline;
            seg.ptStart = this._points.length;
            seg.ptCount = cnt;
            this._points = this._points.concat(splinePoints);
            seg.length = 0;
            for (var i = 1; i < cnt; i++) {
                seg.length += fgui.ToolSet.distance(splinePoints[i - 1].x, splinePoints[i - 1].y, splinePoints[i].x, splinePoints[i].y);
            }
            this._fullLength += seg.length;
            this._segments.push(seg);
            splinePoints.length = 0;
        }
        clear() {
            this._segments.length = 0;
            this._points.length = 0;
        }
        getPointAt(t, result) {
            if (!result)
                result = new Laya.Point();
            else
                result.x = result.y = 0;
            t = fgui.ToolSet.clamp01(t);
            var cnt = this._segments.length;
            if (cnt == 0) {
                return result;
            }
            var seg;
            if (t == 1) {
                seg = this._segments[cnt - 1];
                if (seg.type == fgui.CurveType.Straight) {
                    result.x = fgui.ToolSet.lerp(this._points[seg.ptStart].x, this._points[seg.ptStart + 1].x, t);
                    result.y = fgui.ToolSet.lerp(this._points[seg.ptStart].y, this._points[seg.ptStart + 1].y, t);
                    return result;
                }
                else if (seg.type == fgui.CurveType.Bezier || seg.type == fgui.CurveType.CubicBezier)
                    return this.onBezierCurve(seg.ptStart, seg.ptCount, t, result);
                else
                    return this.onCRSplineCurve(seg.ptStart, seg.ptCount, t, result);
            }
            var len = t * this._fullLength;
            for (var i = 0; i < cnt; i++) {
                seg = this._segments[i];
                len -= seg.length;
                if (len < 0) {
                    t = 1 + len / seg.length;
                    if (seg.type == fgui.CurveType.Straight) {
                        result.x = fgui.ToolSet.lerp(this._points[seg.ptStart].x, this._points[seg.ptStart + 1].x, t);
                        result.y = fgui.ToolSet.lerp(this._points[seg.ptStart].y, this._points[seg.ptStart + 1].y, t);
                    }
                    else if (seg.type == fgui.CurveType.Bezier || seg.type == fgui.CurveType.CubicBezier)
                        result = this.onBezierCurve(seg.ptStart, seg.ptCount, t, result);
                    else
                        result = this.onCRSplineCurve(seg.ptStart, seg.ptCount, t, result);
                    break;
                }
            }
            return result;
        }
        get segmentCount() {
            return this._segments.length;
        }
        getAnchorsInSegment(segmentIndex, points) {
            if (points == null)
                points = new Array();
            var seg = this._segments[segmentIndex];
            for (var i = 0; i < seg.ptCount; i++)
                points.push(new Laya.Point(this._points[seg.ptStart + i].x, this._points[seg.ptStart + i].y));
            return points;
        }
        getPointsInSegment(segmentIndex, t0, t1, points, ts, pointDensity) {
            if (points == null)
                points = new Array();
            if (!pointDensity || isNaN(pointDensity))
                pointDensity = 0.1;
            if (ts)
                ts.push(t0);
            var seg = this._segments[segmentIndex];
            if (seg.type == fgui.CurveType.Straight) {
                points.push(new Laya.Point(fgui.ToolSet.lerp(this._points[seg.ptStart].x, this._points[seg.ptStart + 1].x, t0), fgui.ToolSet.lerp(this._points[seg.ptStart].y, this._points[seg.ptStart + 1].y, t0)));
                points.push(new Laya.Point(fgui.ToolSet.lerp(this._points[seg.ptStart].x, this._points[seg.ptStart + 1].x, t1), fgui.ToolSet.lerp(this._points[seg.ptStart].y, this._points[seg.ptStart + 1].y, t1)));
            }
            else {
                var func;
                if (seg.type == fgui.CurveType.Bezier || seg.type == fgui.CurveType.CubicBezier)
                    func = this.onBezierCurve;
                else
                    func = this.onCRSplineCurve;
                points.push(func.call(this, seg.ptStart, seg.ptCount, t0, new Laya.Point()));
                var SmoothAmount = Math.min(seg.length * pointDensity, 50);
                for (var j = 0; j <= SmoothAmount; j++) {
                    var t = j / SmoothAmount;
                    if (t > t0 && t < t1) {
                        points.push(func.call(this, seg.ptStart, seg.ptCount, t, new Laya.Point()));
                        if (ts)
                            ts.push(t);
                    }
                }
                points.push(func.call(this, seg.ptStart, seg.ptCount, t1, new Laya.Point()));
            }
            if (ts)
                ts.push(t1);
            return points;
        }
        getAllPoints(points, ts, pointDensity) {
            if (points == null)
                points = new Array();
            if (!pointDensity || isNaN(pointDensity))
                pointDensity = 0.1;
            var cnt = this._segments.length;
            for (var i = 0; i < cnt; i++)
                this.getPointsInSegment(i, 0, 1, points, ts, pointDensity);
            return points;
        }
        onCRSplineCurve(ptStart, ptCount, t, result) {
            var adjustedIndex = Math.floor(t * (ptCount - 4)) + ptStart; //Since the equation works with 4 points, we adjust the starting point depending on t to return a point on the specific segment
            var p0x = this._points[adjustedIndex].x;
            var p0y = this._points[adjustedIndex].y;
            var p1x = this._points[adjustedIndex + 1].x;
            var p1y = this._points[adjustedIndex + 1].y;
            var p2x = this._points[adjustedIndex + 2].x;
            var p2y = this._points[adjustedIndex + 2].y;
            var p3x = this._points[adjustedIndex + 3].x;
            var p3y = this._points[adjustedIndex + 3].y;
            var adjustedT = (t == 1) ? 1 : fgui.ToolSet.repeat(t * (ptCount - 4), 1); // Then we adjust t to be that value on that new piece of segment... for t == 1f don't use repeat (that would return 0f);
            var t0 = ((-adjustedT + 2) * adjustedT - 1) * adjustedT * 0.5;
            var t1 = (((3 * adjustedT - 5) * adjustedT) * adjustedT + 2) * 0.5;
            var t2 = ((-3 * adjustedT + 4) * adjustedT + 1) * adjustedT * 0.5;
            var t3 = ((adjustedT - 1) * adjustedT * adjustedT) * 0.5;
            result.x = p0x * t0 + p1x * t1 + p2x * t2 + p3x * t3;
            result.y = p0y * t0 + p1y * t1 + p2y * t2 + p3y * t3;
            return result;
        }
        onBezierCurve(ptStart, ptCount, t, result) {
            var t2 = 1 - t;
            var p0x = this._points[ptStart].x;
            var p0y = this._points[ptStart].y;
            var p1x = this._points[ptStart + 1].x;
            var p1y = this._points[ptStart + 1].y;
            var cp0x = this._points[ptStart + 2].x;
            var cp0y = this._points[ptStart + 2].y;
            if (ptCount == 4) {
                var cp1x = this._points[ptStart + 3].x;
                var cp1y = this._points[ptStart + 3].y;
                result.x = t2 * t2 * t2 * p0x + 3 * t2 * t2 * t * cp0x + 3 * t2 * t * t * cp1x + t * t * t * p1x;
                result.y = t2 * t2 * t2 * p0y + 3 * t2 * t2 * t * cp0y + 3 * t2 * t * t * cp1y + t * t * t * p1y;
            }
            else {
                result.x = t2 * t2 * p0x + 2 * t2 * t * cp0x + t * t * p1x;
                result.y = t2 * t2 * p0y + 2 * t2 * t * cp0y + t * t * p1y;
            }
            return result;
        }
    }
    fgui.GPath = GPath;
    var s_points = new Array();
})(fgui);

(function (fgui) {
    let CurveType;
    (function (CurveType) {
        CurveType[CurveType["CRSpline"] = 0] = "CRSpline";
        CurveType[CurveType["Bezier"] = 1] = "Bezier";
        CurveType[CurveType["CubicBezier"] = 2] = "CubicBezier";
        CurveType[CurveType["Straight"] = 3] = "Straight";
    })(CurveType = fgui.CurveType || (fgui.CurveType = {}));
    class GPathPoint {
        constructor() {
            this.x = 0;
            this.y = 0;
            this.control1_x = 0;
            this.control1_y = 0;
            this.control2_x = 0;
            this.control2_y = 0;
            this.curveType = 0;
        }
        static newPoint(x = 0, y = 0, curveType = 0) {
            var pt = new GPathPoint();
            pt.x = x;
            pt.y = y;
            pt.control1_x = 0;
            pt.control1_y = 0;
            pt.control2_x = 0;
            pt.control2_y = 0;
            pt.curveType = curveType;
            return pt;
        }
        static newBezierPoint(x = 0, y = 0, control1_x = 0, control1_y = 0) {
            var pt = new GPathPoint();
            pt.x = x;
            pt.y = y;
            pt.control1_x = control1_x;
            pt.control1_y = control1_y;
            pt.control2_x = 0;
            pt.control2_y = 0;
            pt.curveType = CurveType.Bezier;
            return pt;
        }
        static newCubicBezierPoint(x = 0, y = 0, control1_x = 0, control1_y = 0, control2_x = 0, control2_y = 0) {
            var pt = new GPathPoint();
            pt.x = x;
            pt.y = y;
            pt.control1_x = control1_x;
            pt.control1_y = control1_y;
            pt.control2_x = control2_x;
            pt.control2_y = control2_y;
            pt.curveType = CurveType.CubicBezier;
            return pt;
        }
        clone() {
            var ret = new GPathPoint();
            ret.x = this.x;
            ret.y = this.y;
            ret.control1_x = this.control1_x;
            ret.control1_y = this.control1_y;
            ret.control2_x = this.control2_x;
            ret.control2_y = this.control2_y;
            ret.curveType = this.curveType;
            return ret;
        }
    }
    fgui.GPathPoint = GPathPoint;
})(fgui);

(function (fgui) {
    class GTween {
        static to(start, end, duration) {
            return fgui.TweenManager.createTween()._to(start, end, duration);
        }
        static to2(start, start2, end, end2, duration) {
            return fgui.TweenManager.createTween()._to2(start, start2, end, end2, duration);
        }
        static to3(start, start2, start3, end, end2, end3, duration) {
            return fgui.TweenManager.createTween()._to3(start, start2, start3, end, end2, end3, duration);
        }
        static to4(start, start2, start3, start4, end, end2, end3, end4, duration) {
            return fgui.TweenManager.createTween()._to4(start, start2, start3, start4, end, end2, end3, end4, duration);
        }
        static toColor(start, end, duration) {
            return fgui.TweenManager.createTween()._toColor(start, end, duration);
        }
        static delayedCall(delay) {
            return fgui.TweenManager.createTween().setDelay(delay);
        }
        static shake(startX, startY, amplitude, duration) {
            return fgui.TweenManager.createTween()._shake(startX, startY, amplitude, duration);
        }
        static isTweening(target, propType) {
            return fgui.TweenManager.isTweening(target, propType);
        }
        static kill(target, complete, propType) {
            fgui.TweenManager.killTweens(target, complete, propType);
        }
        static getTween(target, propType) {
            return fgui.TweenManager.getTween(target, propType);
        }
    }
    GTween.catchCallbackExceptions = true;
    fgui.GTween = GTween;
})(fgui);

(function (fgui) {
    class GTweener {
        constructor() {
            this._startValue = new fgui.TweenValue();
            this._endValue = new fgui.TweenValue();
            this._value = new fgui.TweenValue();
            this._deltaValue = new fgui.TweenValue();
            this._reset();
        }
        setDelay(value) {
            this._delay = value;
            return this;
        }
        get delay() {
            return this._delay;
        }
        setDuration(value) {
            this._duration = value;
            return this;
        }
        get duration() {
            return this._duration;
        }
        setBreakpoint(value) {
            this._breakpoint = value;
            return this;
        }
        setEase(value) {
            this._easeType = value;
            return this;
        }
        setEasePeriod(value) {
            this._easePeriod = value;
            return this;
        }
        setEaseOvershootOrAmplitude(value) {
            this._easeOvershootOrAmplitude = value;
            return this;
        }
        setRepeat(repeat, yoyo = false) {
            this._repeat = repeat;
            this._yoyo = yoyo;
            return this;
        }
        get repeat() {
            return this._repeat;
        }
        setTimeScale(value) {
            this._timeScale = value;
            return this;
        }
        setSnapping(value) {
            this._snapping = value;
            return this;
        }
        setTarget(value, propType) {
            this._target = value;
            this._propType = propType;
            return this;
        }
        get target() {
            return this._target;
        }
        setPath(value) {
            this._path = value;
            return this;
        }
        setUserData(value) {
            this._userData = value;
            return this;
        }
        get userData() {
            return this._userData;
        }
        onUpdate(callback, caller) {
            this._onUpdate = callback;
            this._onUpdateCaller = caller;
            return this;
        }
        onStart(callback, caller) {
            this._onStart = callback;
            this._onStartCaller = caller;
            return this;
        }
        onComplete(callback, caller) {
            this._onComplete = callback;
            this._onCompleteCaller = caller;
            return this;
        }
        get startValue() {
            return this._startValue;
        }
        get endValue() {
            return this._endValue;
        }
        get value() {
            return this._value;
        }
        get deltaValue() {
            return this._deltaValue;
        }
        get normalizedTime() {
            return this._normalizedTime;
        }
        get completed() {
            return this._ended != 0;
        }
        get allCompleted() {
            return this._ended == 1;
        }
        setPaused(paused) {
            this._paused = paused;
            return this;
        }
        /**
         * seek position of the tween, in seconds.
         */
        seek(time) {
            if (this._killed)
                return;
            this._elapsedTime = time;
            if (this._elapsedTime < this._delay) {
                if (this._started)
                    this._elapsedTime = this._delay;
                else
                    return;
            }
            this.update();
        }
        kill(complete) {
            if (this._killed)
                return;
            if (complete) {
                if (this._ended == 0) {
                    if (this._breakpoint >= 0)
                        this._elapsedTime = this._delay + this._breakpoint;
                    else if (this._repeat >= 0)
                        this._elapsedTime = this._delay + this._duration * (this._repeat + 1);
                    else
                        this._elapsedTime = this._delay + this._duration * 2;
                    this.update();
                }
                this.callCompleteCallback();
            }
            this._killed = true;
        }
        _to(start, end, duration) {
            this._valueSize = 1;
            this._startValue.x = start;
            this._endValue.x = end;
            this._value.x = start;
            this._duration = duration;
            return this;
        }
        _to2(start, start2, end, end2, duration) {
            this._valueSize = 2;
            this._startValue.x = start;
            this._endValue.x = end;
            this._startValue.y = start2;
            this._endValue.y = end2;
            this._value.x = start;
            this._value.y = start2;
            this._duration = duration;
            return this;
        }
        _to3(start, start2, start3, end, end2, end3, duration) {
            this._valueSize = 3;
            this._startValue.x = start;
            this._endValue.x = end;
            this._startValue.y = start2;
            this._endValue.y = end2;
            this._startValue.z = start3;
            this._endValue.z = end3;
            this._value.x = start;
            this._value.y = start2;
            this._value.z = start3;
            this._duration = duration;
            return this;
        }
        _to4(start, start2, start3, start4, end, end2, end3, end4, duration) {
            this._valueSize = 4;
            this._startValue.x = start;
            this._endValue.x = end;
            this._startValue.y = start2;
            this._endValue.y = end2;
            this._startValue.z = start3;
            this._endValue.z = end3;
            this._startValue.w = start4;
            this._endValue.w = end4;
            this._value.x = start;
            this._value.y = start2;
            this._value.z = start3;
            this._value.w = start4;
            this._duration = duration;
            return this;
        }
        _toColor(start, end, duration) {
            this._valueSize = 5;
            this._startValue.color = start;
            this._endValue.color = end;
            this._value.color = start;
            this._duration = duration;
            return this;
        }
        _shake(startX, startY, amplitude, duration) {
            this._valueSize = 6;
            this._startValue.x = startX;
            this._startValue.y = startY;
            this._startValue.w = amplitude;
            this._duration = duration;
            return this;
        }
        _init() {
            this._delay = 0;
            this._duration = 0;
            this._breakpoint = -1;
            this._easeType = fgui.EaseType.QuadOut;
            this._timeScale = 1;
            this._easePeriod = 0;
            this._easeOvershootOrAmplitude = 1.70158;
            this._snapping = false;
            this._repeat = 0;
            this._yoyo = false;
            this._valueSize = 0;
            this._started = false;
            this._paused = false;
            this._killed = false;
            this._elapsedTime = 0;
            this._normalizedTime = 0;
            this._ended = 0;
        }
        _reset() {
            this._target = null;
            this._propType = null;
            this._userData = null;
            this._path = null;
            this._onStart = this._onUpdate = this._onComplete = null;
            this._onStartCaller = this._onUpdateCaller = this._onCompleteCaller = null;
        }
        _update(dt) {
            if (this._timeScale != 1)
                dt *= this._timeScale;
            if (dt == 0)
                return;
            if (this._ended != 0) //Maybe completed by seek
             {
                this.callCompleteCallback();
                this._killed = true;
                return;
            }
            this._elapsedTime += dt;
            this.update();
            if (this._ended != 0) {
                if (!this._killed) {
                    this.callCompleteCallback();
                    this._killed = true;
                }
            }
        }
        update() {
            this._ended = 0;
            if (this._valueSize == 0) //DelayedCall
             {
                if (this._elapsedTime >= this._delay + this._duration)
                    this._ended = 1;
                return;
            }
            if (!this._started) {
                if (this._elapsedTime < this._delay)
                    return;
                this._started = true;
                this.callStartCallback();
                if (this._killed)
                    return;
            }
            var reversed = false;
            var tt = this._elapsedTime - this._delay;
            if (this._breakpoint >= 0 && tt >= this._breakpoint) {
                tt = this._breakpoint;
                this._ended = 2;
            }
            if (this._repeat != 0) {
                var round = Math.floor(tt / this._duration);
                tt -= this._duration * round;
                if (this._yoyo)
                    reversed = round % 2 == 1;
                if (this._repeat > 0 && this._repeat - round < 0) {
                    if (this._yoyo)
                        reversed = this._repeat % 2 == 1;
                    tt = this._duration;
                    this._ended = 1;
                }
            }
            else if (tt >= this._duration) {
                tt = this._duration;
                this._ended = 1;
            }
            this._normalizedTime = fgui.evaluateEase(this._easeType, reversed ? (this._duration - tt) : tt, this._duration, this._easeOvershootOrAmplitude, this._easePeriod);
            this._value.setZero();
            this._deltaValue.setZero();
            if (this._valueSize == 6) {
                if (this._ended == 0) {
                    var r = this._startValue.w * (1 - this._normalizedTime);
                    var rx = r * (Math.random() > 0.5 ? 1 : -1);
                    var ry = r * (Math.random() > 0.5 ? 1 : -1);
                    this._deltaValue.x = rx;
                    this._deltaValue.y = ry;
                    this._value.x = this._startValue.x + rx;
                    this._value.y = this._startValue.y + ry;
                }
                else {
                    this._value.x = this._startValue.x;
                    this._value.y = this._startValue.y;
                }
            }
            else if (this._path) {
                var pt = s_vec2;
                this._path.getPointAt(this._normalizedTime, pt);
                if (this._snapping) {
                    pt.x = Math.round(pt.x);
                    pt.y = Math.round(pt.y);
                }
                this._deltaValue.x = pt.x - this._value.x;
                this._deltaValue.y = pt.y - this._value.y;
                this._value.x = pt.x;
                this._value.y = pt.y;
            }
            else {
                let cnt = Math.min(this._valueSize, 4);
                for (var i = 0; i < cnt; i++) {
                    var n1 = this._startValue.getField(i);
                    var n2 = this._endValue.getField(i);
                    var f = n1 + (n2 - n1) * this._normalizedTime;
                    if (this._snapping)
                        f = Math.round(f);
                    this._deltaValue.setField(i, f - this._value.getField(i));
                    this._value.setField(i, f);
                }
            }
            if (this._target && this._propType) {
                if (this._propType instanceof Function) {
                    switch (this._valueSize) {
                        case 1:
                            this._propType.call(this._target, this._value.x);
                            break;
                        case 2:
                            this._propType.call(this._target, this._value.x, this._value.y);
                            break;
                        case 3:
                            this._propType.call(this._target, this._value.x, this._value.y, this._value.z);
                            break;
                        case 4:
                            this._propType.call(this._target, this._value.x, this._value.y, this._value.z, this._value.w);
                            break;
                        case 5:
                            this._propType.call(this._target, this._value.color);
                            break;
                        case 6:
                            this._propType.call(this._target, this._value.x, this._value.y);
                            break;
                    }
                }
                else {
                    if (this._valueSize == 5)
                        this._target[this._propType] = this._value.color;
                    else
                        this._target[this._propType] = this._value.x;
                }
            }
            this.callUpdateCallback();
        }
        callStartCallback() {
            if (this._onStart != null) {
                try {
                    this._onStart.call(this._onStartCaller, this);
                }
                catch (err) {
                    console.warn("FairyGUI: error in start callback", err);
                }
            }
        }
        callUpdateCallback() {
            if (this._onUpdate != null) {
                try {
                    this._onUpdate.call(this._onUpdateCaller, this);
                }
                catch (err) {
                    console.warn("FairyGUI: error in update callback", err);
                }
            }
        }
        callCompleteCallback() {
            if (this._onComplete != null) {
                try {
                    this._onComplete.call(this._onCompleteCaller, this);
                }
                catch (err) {
                    console.warn("FairyGUI: error in complete callback", err);
                }
            }
        }
    }
    fgui.GTweener = GTweener;
    var s_vec2 = new Laya.Point();
})(fgui);

(function (fgui) {
    class TweenManager {
        static createTween() {
            if (!_inited) {
                Laya.timer.frameLoop(1, null, TweenManager.update);
                _inited = true;
            }
            var tweener;
            var cnt = _tweenerPool.length;
            if (cnt > 0) {
                tweener = _tweenerPool.pop();
            }
            else
                tweener = new fgui.GTweener();
            tweener._init();
            _activeTweens[_totalActiveTweens++] = tweener;
            return tweener;
        }
        static isTweening(target, propType) {
            if (target == null)
                return false;
            var anyType = !propType;
            for (var i = 0; i < _totalActiveTweens; i++) {
                var tweener = _activeTweens[i];
                if (tweener && tweener.target == target && !tweener._killed
                    && (anyType || tweener._propType == propType))
                    return true;
            }
            return false;
        }
        static killTweens(target, completed, propType) {
            if (target == null)
                return false;
            var flag = false;
            var cnt = _totalActiveTweens;
            var anyType = !propType;
            for (var i = 0; i < cnt; i++) {
                var tweener = _activeTweens[i];
                if (tweener && tweener.target == target && !tweener._killed
                    && (anyType || tweener._propType == propType)) {
                    tweener.kill(completed);
                    flag = true;
                }
            }
            return flag;
        }
        static getTween(target, propType) {
            if (target == null)
                return null;
            var cnt = _totalActiveTweens;
            var anyType = !propType;
            for (var i = 0; i < cnt; i++) {
                var tweener = _activeTweens[i];
                if (tweener && tweener.target == target && !tweener._killed
                    && (anyType || tweener._propType == propType)) {
                    return tweener;
                }
            }
            return null;
        }
        static update() {
            var dt = Laya.timer.delta / 1000;
            var cnt = _totalActiveTweens;
            var freePosStart = -1;
            for (var i = 0; i < cnt; i++) {
                var tweener = _activeTweens[i];
                if (tweener == null) {
                    if (freePosStart == -1)
                        freePosStart = i;
                }
                else if (tweener._killed) {
                    tweener._reset();
                    _tweenerPool.push(tweener);
                    _activeTweens[i] = null;
                    if (freePosStart == -1)
                        freePosStart = i;
                }
                else {
                    if ((tweener._target instanceof fgui.GObject) && tweener._target.isDisposed)
                        tweener._killed = true;
                    else if (!tweener._paused)
                        tweener._update(dt);
                    if (freePosStart != -1) {
                        _activeTweens[freePosStart] = tweener;
                        _activeTweens[i] = null;
                        freePosStart++;
                    }
                }
            }
            if (freePosStart >= 0) {
                if (_totalActiveTweens != cnt) //new tweens added
                 {
                    var j = cnt;
                    cnt = _totalActiveTweens - cnt;
                    for (i = 0; i < cnt; i++)
                        _activeTweens[freePosStart++] = _activeTweens[j++];
                }
                _totalActiveTweens = freePosStart;
            }
        }
    }
    fgui.TweenManager = TweenManager;
    var _activeTweens = [];
    var _tweenerPool = [];
    var _totalActiveTweens = 0;
    var _inited = false;
})(fgui);

(function (fgui) {
    class TweenValue {
        constructor() {
            this.x = this.y = this.z = this.w = 0;
        }
        get color() {
            return (this.w << 24) + (this.x << 16) + (this.y << 8) + this.z;
        }
        set color(value) {
            this.x = (value & 0xFF0000) >> 16;
            this.y = (value & 0x00FF00) >> 8;
            this.z = (value & 0x0000FF);
            this.w = (value & 0xFF000000) >> 24;
        }
        getField(index) {
            switch (index) {
                case 0:
                    return this.x;
                case 1:
                    return this.y;
                case 2:
                    return this.z;
                case 3:
                    return this.w;
                default:
                    throw new Error("Index out of bounds: " + index);
            }
        }
        setField(index, value) {
            switch (index) {
                case 0:
                    this.x = value;
                    break;
                case 1:
                    this.y = value;
                    break;
                case 2:
                    this.z = value;
                    break;
                case 3:
                    this.w = value;
                    break;
                default:
                    throw new Error("Index out of bounds: " + index);
            }
        }
        setZero() {
            this.x = this.y = this.z = this.w = 0;
        }
    }
    fgui.TweenValue = TweenValue;
})(fgui);

(function (fgui) {
    class ByteBuffer extends Laya.Byte {
        constructor(data, offset, length) {
            offset = offset || 0;
            if (length == null || length == -1)
                length = data.byteLength - offset;
            if (offset == 0 && length == data.byteLength)
                super(data);
            else {
                super();
                this._u8d_ = new Uint8Array(data, offset, length);
                this._d_ = new DataView(this._u8d_.buffer, offset, length);
                this._length = length;
            }
            this.endian = Laya.Byte.BIG_ENDIAN;
        }
        skip(count) {
            this.pos += count;
        }
        readBool() {
            return this.getUint8() == 1;
        }
        readS() {
            var index = this.getUint16();
            if (index == 65534) //null
                return null;
            else if (index == 65533)
                return "";
            else
                return this.stringTable[index];
        }
        readSArray(cnt) {
            var ret = new Array(cnt);
            for (var i = 0; i < cnt; i++)
                ret[i] = this.readS();
            return ret;
        }
        writeS(value) {
            var index = this.getUint16();
            if (index != 65534 && index != 65533)
                this.stringTable[index] = value;
        }
        readColor(hasAlpha) {
            var r = this.getUint8();
            var g = this.getUint8();
            var b = this.getUint8();
            var a = this.getUint8();
            return (hasAlpha ? (a << 24) : 0) + (r << 16) + (g << 8) + b;
        }
        readColorS(hasAlpha) {
            var r = this.getUint8();
            var g = this.getUint8();
            var b = this.getUint8();
            var a = this.getUint8();
            if (hasAlpha && a != 255)
                return "rgba(" + r + "," + g + "," + b + "," + (a / 255) + ")";
            else {
                var sr = r.toString(16);
                var sg = g.toString(16);
                var sb = b.toString(16);
                if (sr.length == 1)
                    sr = "0" + sr;
                if (sg.length == 1)
                    sg = "0" + sg;
                if (sb.length == 1)
                    sb = "0" + sb;
                return "#" + sr + sg + sb;
            }
        }
        readChar() {
            var i = this.getUint16();
            return String.fromCharCode(i);
        }
        readBuffer() {
            var count = this.getUint32();
            var ba = new ByteBuffer(this.buffer, this._pos_, count);
            this.pos += count;
            ba.stringTable = this.stringTable;
            ba.version = this.version;
            return ba;
        }
        seek(indexTablePos, blockIndex) {
            var tmp = this._pos_;
            this.pos = indexTablePos;
            var segCount = this.getUint8();
            if (blockIndex < segCount) {
                var useShort = this.getUint8() == 1;
                var newPos;
                if (useShort) {
                    this.pos += 2 * blockIndex;
                    newPos = this.getUint16();
                }
                else {
                    this.pos += 4 * blockIndex;
                    newPos = this.getUint32();
                }
                if (newPos > 0) {
                    this.pos = indexTablePos + newPos;
                    return true;
                }
                else {
                    this.pos = tmp;
                    return false;
                }
            }
            else {
                this.pos = tmp;
                return false;
            }
        }
    }
    fgui.ByteBuffer = ByteBuffer;
})(fgui);

(function (fgui) {
    let _func = Laya.HitArea["_isHitGraphic"];
    class ChildHitArea extends Laya.HitArea {
        constructor(child, reversed) {
            super();
            this._child = child;
            this._reversed = reversed;
            if (this._reversed)
                this.unHit = child.hitArea.hit;
            else
                this.hit = child.hitArea.hit;
        }
        contains(x, y, sp) {
            var tPos;
            tPos = Laya.Point.TEMP;
            tPos.setTo(0, 0);
            tPos = this._child.toParentPoint(tPos);
            if (this._reversed)
                return !_func(x - tPos.x, y - tPos.y, sp, this.unHit);
            else
                return _func(x - tPos.x, y - tPos.y, sp, this.hit);
        }
    }
    fgui.ChildHitArea = ChildHitArea;
})(fgui);

(function (fgui) {
    class ColorMatrix {
        constructor(p_brightness, p_contrast, p_saturation, p_hue) {
            this.matrix = new Array(LENGTH);
            this.reset();
            if (p_brightness !== undefined || p_contrast !== undefined || p_saturation !== undefined || p_hue !== undefined)
                this.adjustColor(p_brightness, p_contrast, p_saturation, p_hue);
        }
        reset() {
            for (var i = 0; i < LENGTH; i++) {
                this.matrix[i] = IDENTITY_MATRIX[i];
            }
        }
        invert() {
            this.multiplyMatrix([-1, 0, 0, 0, 255,
                0, -1, 0, 0, 255,
                0, 0, -1, 0, 255,
                0, 0, 0, 1, 0]);
        }
        adjustColor(p_brightness, p_contrast, p_saturation, p_hue) {
            this.adjustHue(p_hue || 0);
            this.adjustContrast(p_contrast || 0);
            this.adjustBrightness(p_brightness || 0);
            this.adjustSaturation(p_saturation || 0);
        }
        adjustBrightness(p_val) {
            p_val = this.cleanValue(p_val, 1) * 255;
            this.multiplyMatrix([
                1, 0, 0, 0, p_val,
                0, 1, 0, 0, p_val,
                0, 0, 1, 0, p_val,
                0, 0, 0, 1, 0
            ]);
        }
        adjustContrast(p_val) {
            p_val = this.cleanValue(p_val, 1);
            var s = p_val + 1;
            var o = 128 * (1 - s);
            this.multiplyMatrix([
                s, 0, 0, 0, o,
                0, s, 0, 0, o,
                0, 0, s, 0, o,
                0, 0, 0, 1, 0
            ]);
        }
        adjustSaturation(p_val) {
            p_val = this.cleanValue(p_val, 1);
            p_val += 1;
            var invSat = 1 - p_val;
            var invLumR = invSat * LUMA_R;
            var invLumG = invSat * LUMA_G;
            var invLumB = invSat * LUMA_B;
            this.multiplyMatrix([
                (invLumR + p_val), invLumG, invLumB, 0, 0,
                invLumR, (invLumG + p_val), invLumB, 0, 0,
                invLumR, invLumG, (invLumB + p_val), 0, 0,
                0, 0, 0, 1, 0
            ]);
        }
        adjustHue(p_val) {
            p_val = this.cleanValue(p_val, 1);
            p_val *= Math.PI;
            var cos = Math.cos(p_val);
            var sin = Math.sin(p_val);
            this.multiplyMatrix([
                ((LUMA_R + (cos * (1 - LUMA_R))) + (sin * -(LUMA_R))), ((LUMA_G + (cos * -(LUMA_G))) + (sin * -(LUMA_G))), ((LUMA_B + (cos * -(LUMA_B))) + (sin * (1 - LUMA_B))), 0, 0,
                ((LUMA_R + (cos * -(LUMA_R))) + (sin * 0.143)), ((LUMA_G + (cos * (1 - LUMA_G))) + (sin * 0.14)), ((LUMA_B + (cos * -(LUMA_B))) + (sin * -0.283)), 0, 0,
                ((LUMA_R + (cos * -(LUMA_R))) + (sin * -((1 - LUMA_R)))), ((LUMA_G + (cos * -(LUMA_G))) + (sin * LUMA_G)), ((LUMA_B + (cos * (1 - LUMA_B))) + (sin * LUMA_B)), 0, 0,
                0, 0, 0, 1, 0
            ]);
        }
        concat(p_matrix) {
            if (p_matrix.length != LENGTH) {
                return;
            }
            this.multiplyMatrix(p_matrix);
        }
        clone() {
            var result = new ColorMatrix();
            result.copyMatrix(this.matrix);
            return result;
        }
        copyMatrix(p_matrix) {
            var l = LENGTH;
            for (var i = 0; i < l; i++) {
                this.matrix[i] = p_matrix[i];
            }
        }
        multiplyMatrix(p_matrix) {
            var col = [];
            var i = 0;
            for (var y = 0; y < 4; ++y) {
                for (var x = 0; x < 5; ++x) {
                    col[i + x] = p_matrix[i] * this.matrix[x] +
                        p_matrix[i + 1] * this.matrix[x + 5] +
                        p_matrix[i + 2] * this.matrix[x + 10] +
                        p_matrix[i + 3] * this.matrix[x + 15] +
                        (x == 4 ? p_matrix[i + 4] : 0);
                }
                i += 5;
            }
            this.copyMatrix(col);
        }
        cleanValue(p_val, p_limit) {
            return Math.min(p_limit, Math.max(-p_limit, p_val));
        }
    }
    fgui.ColorMatrix = ColorMatrix;
    // identity matrix constant:
    const IDENTITY_MATRIX = [
        1, 0, 0, 0, 0,
        0, 1, 0, 0, 0,
        0, 0, 1, 0, 0,
        0, 0, 0, 1, 0
    ];
    const LENGTH = IDENTITY_MATRIX.length;
    const LUMA_R = 0.299;
    const LUMA_G = 0.587;
    const LUMA_B = 0.114;
})(fgui);

(function (fgui) {
    class PixelHitTest extends Laya.HitArea {
        constructor(data, offsetX, offsetY) {
            super();
            this._data = data;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.scaleX = 1;
            this.scaleY = 1;
        }
        contains(x, y) {
            x = Math.floor((x / this.scaleX - this.offsetX) * this._data.scale);
            y = Math.floor((y / this.scaleY - this.offsetY) * this._data.scale);
            if (x < 0 || y < 0 || x >= this._data.pixelWidth)
                return false;
            var pos = y * this._data.pixelWidth + x;
            var pos2 = Math.floor(pos / 8);
            var pos3 = pos % 8;
            if (pos2 >= 0 && pos2 < this._data.pixels.length)
                return ((this._data.pixels[pos2] >> pos3) & 0x1) == 1;
            else
                return false;
        }
    }
    fgui.PixelHitTest = PixelHitTest;
    class PixelHitTestData {
        constructor() {
        }
        load(ba) {
            ba.getInt32();
            this.pixelWidth = ba.getInt32();
            this.scale = 1 / ba.readByte();
            var len = ba.getInt32();
            this.pixels = [];
            for (var i = 0; i < len; i++) {
                var j = ba.readByte();
                if (j < 0)
                    j += 256;
                this.pixels[i] = j;
            }
        }
    }
    fgui.PixelHitTestData = PixelHitTestData;
})(fgui);

(function (fgui) {
    class UBBParser extends Laya.UBBParser {
    }
    fgui.UBBParser = UBBParser;
})(fgui);
///<reference path="UBBParser.ts"/>

///<reference path="UBBParser.ts"/>
(function (fgui) {
    class ToolSet {
        static startsWith(source, str, ignoreCase) {
            if (!source)
                return false;
            else if (source.length < str.length)
                return false;
            else {
                source = source.substring(0, str.length);
                if (!ignoreCase)
                    return source == str;
                else
                    return source.toLowerCase() == str.toLowerCase();
            }
        }
        static endsWith(source, str, ignoreCase) {
            if (!source)
                return false;
            else if (source.length < str.length)
                return false;
            else {
                source = source.substring(source.length - str.length);
                if (!ignoreCase)
                    return source == str;
                else
                    return source.toLowerCase() == str.toLowerCase();
            }
        }
        static trimRight(targetString) {
            var tempChar = "";
            for (var i = targetString.length - 1; i >= 0; i--) {
                tempChar = targetString.charAt(i);
                if (tempChar != " " && tempChar != "\n" && tempChar != "\r") {
                    break;
                }
            }
            return targetString.substring(0, i + 1);
        }
        static convertToHtmlColor(argb, hasAlpha) {
            var alpha;
            if (hasAlpha)
                alpha = (argb >> 24 & 0xFF).toString(16);
            else
                alpha = "";
            var red = (argb >> 16 & 0xFF).toString(16);
            var green = (argb >> 8 & 0xFF).toString(16);
            var blue = (argb & 0xFF).toString(16);
            if (alpha.length == 1)
                alpha = "0" + alpha;
            if (red.length == 1)
                red = "0" + red;
            if (green.length == 1)
                green = "0" + green;
            if (blue.length == 1)
                blue = "0" + blue;
            return "#" + alpha + red + green + blue;
        }
        static convertFromHtmlColor(str, hasAlpha) {
            if (str.length < 1)
                return 0;
            if (str.charAt(0) == "#")
                str = str.substr(1);
            if (str.length == 8)
                return (parseInt(str.substr(0, 2), 16) << 24) + parseInt(str.substr(2), 16);
            else if (hasAlpha)
                return 0xFF000000 + parseInt(str, 16);
            else
                return parseInt(str, 16);
        }
        static displayObjectToGObject(obj) {
            while (obj && !(obj instanceof Laya.Stage)) {
                if (obj["$owner"])
                    return obj["$owner"];
                obj = obj.parent;
            }
            return null;
        }
        static encodeHTML(str) {
            if (!str)
                return "";
            else
                return str.replace(/&/g, "&amp;").replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;").replace(/'/g, "&apos;").replace(/"/g, "&quot;");
        }
        static clamp(value, min, max) {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }
        static clamp01(value) {
            if (isNaN(value))
                value = 0;
            else if (value > 1)
                value = 1;
            else if (value < 0)
                value = 0;
            return value;
        }
        static lerp(start, end, percent) {
            return (start + percent * (end - start));
        }
        static repeat(t, length) {
            return t - Math.floor(t / length) * length;
        }
        static distance(x1, y1, x2, y2) {
            return Math.sqrt(Math.pow(x1 - x2, 2) + Math.pow(y1 - y2, 2));
        }
        static setColorFilter(obj, color) {
            var filter = obj.$_colorFilter_; //cached instance
            var filters = obj.filters;
            var toApplyColor;
            var toApplyGray;
            var tp = typeof (color);
            if (tp == "boolean") //gray
             {
                toApplyColor = filter ? filter.$_color_ : null;
                toApplyGray = color;
            }
            else {
                if (tp == "string") {
                    var arr = Laya.ColorUtils.create(color).arrColor;
                    if (arr[0] == 1 && arr[1] == 1 && arr[2] == 1)
                        color = null;
                    else {
                        color = [arr[0], 0, 0, 0, 0,
                            0, arr[1], 0, 0, 0,
                            0, 0, arr[2], 0, 0,
                            0, 0, 0, 1, 0];
                    }
                }
                toApplyColor = color;
                toApplyGray = filter ? filter.$_grayed_ : false;
            }
            if ((!toApplyColor && toApplyColor != 0) && !toApplyGray) {
                if (filters && filter) {
                    let i = filters.indexOf(filter);
                    if (i != -1) {
                        filters.splice(i, 1);
                        if (filters.length > 0)
                            obj.filters = filters;
                        else
                            obj.filters = null;
                    }
                }
                return;
            }
            if (!filter) {
                filter = new Laya.ColorFilter();
                obj.$_colorFilter_ = filter;
            }
            if (!filters)
                filters = [filter];
            else {
                let i = filters.indexOf(filter);
                if (i == -1)
                    filters.push(filter);
            }
            obj.filters = filters;
            filter.$_color_ = toApplyColor;
            filter.$_grayed_ = toApplyGray;
            filter.reset();
            if (toApplyGray)
                filter.gray();
            else if (toApplyColor.length == 20)
                filter.setByMatrix(toApplyColor);
            else
                filter.setByMatrix(getColorMatrix(toApplyColor[0], toApplyColor[1], toApplyColor[2], toApplyColor[3]));
        }
    }
    fgui.ToolSet = ToolSet;
    let helper = new fgui.ColorMatrix();
    function getColorMatrix(p_brightness, p_contrast, p_saturation, p_hue, result) {
        result = result || new Array(fgui.ColorMatrix.length);
        helper.reset();
        helper.adjustColor(p_brightness, p_contrast, p_saturation, p_hue);
        helper.matrix.forEach((e, i) => result[i] = e);
        return result;
    }
})(fgui);

//# sourceMappingURL=fairygui.js.map
