class SaveAjaxPlugin {
    constructor(editor) {
        this.editor = editor;
        this.saveScheduler = new SaveScheduler(
            () => this.saveDraftReply(), 
            (dirty) => this.onDirtyChanged(dirty),
            (saveState) => this.onSaveStateChanged(saveState),
            this.getAutosaveInterval());
        
        this.setupEditor();
        this.setupOnBeforeUnloadEvent();
        this.setupFinalSubmitEvent();
        
        this.saveScheduler.start();
    }
    
    setupEditor() {
        let self = this;
        this.editor.on('change', () => this.saveScheduler.onChanged());
        this.editor.addButton('saveajax', {
            text: 'Save',
            icon: 'save',
            disabled: true,
            onclick: () => this.saveScheduler.explicitSave(),
            onPostRender: function () { self.saveButton = this; }
        });
        
        this.editor.on('init', () => {
            let statusbar = this.editor.theme.panel && this.editor.theme.panel.find('#statusbar')[0];
            if (statusbar) {
                statusbar.insert({
                    type: 'label',
                    name: 'lastSaved',
                    text: [""],
                    style: 'float: left',
                    classes: 'path',
                    disabled: this.editor.settings.readonly
                }, 0);
                statusbar.insert({
                    type: 'label',
                    name: 'savingStatus',
                    text: [""],
                    style: 'float: right',
                    classes: 'path',
                    disabled: this.editor.settings.readonly
                }, 1);
            }
        });
    }

    saveDraftReply() {
        let saveDraft = this.editor.getParam("saveajax_save_draft_callback");
        return prolongPromise(saveDraft());
    }

    setupOnBeforeUnloadEvent() {
        $(window).on('beforeunload', () => {
            if (this.saveScheduler.dirty ||
                this.saveScheduler.saveState === SaveState.PENDING) {
                return "You have unsaved changes. Are you sure you want to leave?";
            }
        });
    }

    setupFinalSubmitEvent() {
        let formToSubmit = this.editor.formElement;
        $(formToSubmit).on("submit", (e) => {
            $(formToSubmit).off("submit");
            $(formToSubmit).find(':submit').prop("disabled", true);
            this.editor.setProgressState(true);
            this.saveScheduler.pauseAndWaitForCompletion()
                .then(() => {
                    $(window).off('beforeunload');
                    let finalSubmit = this.editor.getParam("saveajax_final_submit_callback");
                    return finalSubmit();
                })
                .then(() => {
                    location = self.location;
                })
                .catch((error) => {
                    $(formToSubmit).find(':submit').prop("disabled", false);
                    this.editor.setProgressState(false);
                    this.setupOnBeforeUnloadEvent();
                    this.setupFinalSubmitEvent();
                    this.saveScheduler.resume();
                    if (error.html) {
                        alertDialogHtml("Faild to submit", error.html, 600 /* width */);
                    } else {
                        alertDialogText("Faild to submit", error.text || "Submission failed.", 600 /* width */);
                    }
                });
            e.preventDefault();
        });
    }
    
    onDirtyChanged(dirty) {
        this.saveButton.disabled(!dirty);
    }
    
    onSaveStateChanged(saveState) {
        let savingStatusElts = this.editor.theme.panel.find('#savingStatus');
        if (!savingStatusElts || savingStatusElts.length === 0) {
            return;
        }
        let lastSavedElts = this.editor.theme.panel.find('#lastSaved');
        if (!lastSavedElts || lastSavedElts.length === 0) {
            return;
        }
        let savingStatus = savingStatusElts[0].getEl();
        let lastSaved = lastSavedElts[0].getEl();
        
        if (saveState === SaveState.SUCCESS) {
            $(savingStatus)
                .text("")
                .css('color', 'black')
                .css('font-weight', 'normal');
            $(lastSaved)
                .text(this.getLastSavedText(new Date()));
        } else if (saveState === SaveState.FAILURE) {
            $(savingStatus)
                .text("Save failed.")
                .css('color', 'red')
                .css('font-weight', 'bold');
            return "Save failed.";
        } else if (saveState === SaveState.PENDING) {
            $(savingStatus)
                .text("Saving...")
                .css('color', 'black')
                .css('font-weight', 'normal');
        } else {
            throw new Error("Invalid save state.");
        }
    }
    
    getLastSavedText(dateTime) {
        return "Last saved: " + 
            dateTime.toLocaleDateString() + 
            " " + 
            dateTime.toLocaleTimeString();
    }
    
    getAutosaveInterval() {
        let autosaveInterval = this.editor.getParam("saveajax_autosave_interval");
        if (!autosaveInterval) {
            autosaveInterval = 60000; // 1 minute
        }
        return autosaveInterval;
    }
}

const SaveState = Object.freeze({
    PENDING: Symbol("pending"),
    SUCCESS: Symbol("success"),
    FAILURE: Symbol("failure")
});

class SaveScheduler {
    constructor(saveFunc, notifyDirtyFunc, notifySaveStateFunc, autoSaveInterval) {
        this.saveFunc = saveFunc;
        this.notifyDirtyFunc = notifyDirtyFunc;
        this.notifySaveStateFunc = notifySaveStateFunc;
        this.explicitSaveRequested = false;
        this.dirty = false;
        this.saveState = SaveState.SUCCESS;
        this.scheduler = new AsyncFunctionScheduler(
            () => this._saveIfDirty(),
            autoSaveInterval);
    }

    onChanged() {
        this._setDirty(true);
    }

    start() {
        this.scheduler.start();
    }

    explicitSave() {
        this.explicitSaveRequested = true;
        this.scheduler.interruptTimeoutIfWaiting();
    }

    pauseAndWaitForCompletion() {
        return this.scheduler.pauseAndWaitForCompletion();
    }
    
    resume() {
        this.scheduler.resume();
    }

    _setDirty(dirty) {
        let changed = (dirty !== this.dirty);
        this.dirty = dirty;
        if (changed) {
            this.notifyDirtyFunc(dirty);
        }
    }
    
    _setSaveState(saveState) {
        let changed = (saveState !== this.saveState);
        this.saveState = saveState;
        if (changed) {
            this.notifySaveStateFunc(saveState);
        }
    }

    _saveIfDirty() {
        this.explicitSaveRequested = false;
        if (!this.dirty) {
            return Promise.resolve();
        }
        this._setDirty(false);
        this._setSaveState(SaveState.PENDING);
        return this.saveFunc()
            .then(() => {
                this._setSaveState(SaveState.SUCCESS);
            })
            .catch(() => {
                this._setSaveState(SaveState.FAILURE);
                this._setDirty(true);
            })
            .then(() => {
                if (this.explicitSaveRequested) {
                    return this._saveIfDirty();
                }
            })
    }
}

class AsyncFunctionScheduler {
    constructor(func, interval) {
        this.func = func;
        this.funcCompleted = Promise.resolve();
        this.interruptibleTimeout = new InterruptibleTimeout(interval);
        this.stopped = false;
    }

    start() {
        this._scheduleFunction();
    }

    pauseAndWaitForCompletion() {
        this.stopped = true;
        return this.funcCompleted;
    }
    
    resume() {
        this.stopped = false;
    }

    interruptTimeoutIfWaiting() {
        this.interruptibleTimeout.interrupt();
    }

    _scheduleFunction() {
        this.interruptibleTimeout.waitForTimeoutOrInterrupt()
            .then(() => {
                if (this.stopped) {
                    return Promise.resolve();
                }
                this.funcCompleted = this.func();
                return this.funcCompleted;
            })
            .then(() => {
                this.funcCompleted = Promise.resolve();
                this._scheduleFunction();
            });
    }
}

class InterruptibleTimeout {
    constructor(timeout) {
        this.timeout = timeout;
        this.resolve = () => {}
    }

    waitForTimeoutOrInterrupt() {
        this.promise = new Promise((resolve, reject) => {
            this.resolve = resolve;
            setTimeout(() => resolve(), this.timeout)
        });
        return this.promise;
    }

    interrupt() {
        this.resolve();
    }
}

tinymce.PluginManager.add('saveajax', SaveAjaxPlugin);