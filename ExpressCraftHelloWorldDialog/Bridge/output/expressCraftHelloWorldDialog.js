/**
 * @version 1.0.0.0
 * @copyright Copyright ©  2017
 * @compiler Bridge.NET 15.6.0
 */
Bridge.assembly("ExpressCraftHelloWorldDialog", function ($asm, globals) {
    "use strict";

    Bridge.define("ExpressCraftHelloWorldDialog.App", {
        statics: {
            r: null,
            config: {
                init: function () {
                    this.r = new System.Random.ctor();
                }
            },
            getRandomText: function () {
                var builder = new System.Text.StringBuilder();

                for (var i = 0; i < 1000; i = (i + 1) | 0) {
                    builder.append(ExpressCraftHelloWorldDialog.App.r.next().toString());
                }
                return builder.toString();
            }
        },
        $main: function () {
            var $t, $t1, $t2, $t3;
            // Setup the form events and containers*
            ExpressCraft.Form.setup();

            var errorBtn = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: ($t=ExpressCraft.MessageBoxLayout.Error, System.Enum.format(ExpressCraft.MessageBoxLayout, $t, "G")),
                itemClick: $asm.$.ExpressCraftHelloWorldDialog.App.f1
            } );
            errorBtn.content.style.position = "relative";

            var exclamationBtn = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: ($t1=ExpressCraft.MessageBoxLayout.Exclamation, System.Enum.format(ExpressCraft.MessageBoxLayout, $t1, "G")),
                itemClick: $asm.$.ExpressCraftHelloWorldDialog.App.f2
            } );
            exclamationBtn.content.style.position = "relative";

            var informationBtn = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: ($t2=ExpressCraft.MessageBoxLayout.Information, System.Enum.format(ExpressCraft.MessageBoxLayout, $t2, "G")),
                itemClick: $asm.$.ExpressCraftHelloWorldDialog.App.f3
            } );
            informationBtn.content.style.position = "relative";

            var questionBtn = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: ($t3=ExpressCraft.MessageBoxLayout.Question, System.Enum.format(ExpressCraft.MessageBoxLayout, $t3, "G")),
                itemClick: $asm.$.ExpressCraftHelloWorldDialog.App.f4
            } );
            questionBtn.content.style.position = "relative";

            var informationBtn2 = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: "Large Information",
                itemClick: $asm.$.ExpressCraftHelloWorldDialog.App.f5
            } );
            informationBtn2.content.style.position = "relative";
            informationBtn2.content.style.width = "auto";

            ExpressCraft.Helper.appendChildrenTabIndex(ExpressCraft.Form.getWindowHolder(), [errorBtn, exclamationBtn, informationBtn, questionBtn, informationBtn2]);
        }
    });

    Bridge.ns("ExpressCraftHelloWorldDialog.App", $asm.$);

    Bridge.apply($asm.$.ExpressCraftHelloWorldDialog.App, {
        f1: function (ev) {
            (new ExpressCraft.MessageBoxForm.ctor("Hello World!", ExpressCraft.MessageBoxLayout.Error)).showDialog();
        },
        f2: function (ev) {
            (new ExpressCraft.MessageBoxForm.ctor("Hello World!", ExpressCraft.MessageBoxLayout.Exclamation)).showDialog();
        },
        f3: function (ev) {
            (new ExpressCraft.MessageBoxForm.ctor("Hello World!", ExpressCraft.MessageBoxLayout.Information)).showDialog();
        },
        f4: function (ev) {
            (new ExpressCraft.MessageBoxForm.ctor("Hello World!", ExpressCraft.MessageBoxLayout.Question)).showDialog();
        },
        f5: function (ev) {
            (new ExpressCraft.MessageBoxForm.ctor(ExpressCraftHelloWorldDialog.App.getRandomText(), ExpressCraft.MessageBoxLayout.Information)).showDialog();
        }
    });
});
