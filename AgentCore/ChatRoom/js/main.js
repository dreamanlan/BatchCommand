// ============================================================================
// AiClaw - Main entry point
// ============================================================================
const config = {
    type: Phaser.AUTO,
    scale: {
        mode: Phaser.Scale.RESIZE,
        parent: 'app-container',
        width: '100%',
        height: '100%'
    },
    backgroundColor: '#2d2d2d',
    scene: {
        preload: preload,
        create: create,
        update: update
    }
};

const app = new Phaser.Game(config);

function preload() {
}

function create() {
    this.infoText = this.add.text(
        this.cameras.main.centerX,
        this.cameras.main.centerY,
        'AiClaw Canvas\n(visualization area)',
        { font: '24px Arial', fill: '#555555', align: 'center' }
    );
    this.infoText.setOrigin(0.5, 0.5);

    this.scale.on('resize', (gameSize) => {
        this.cameras.resize(gameSize.width, gameSize.height);
        this.infoText.setPosition(gameSize.width / 2, gameSize.height / 2);
    });
}

function update() {
}

// Initialize chat room after DOM is ready
const relay = new RelayClient();
const chatRoom = new ChatRoom(relay);

// Load encrypted secrets (apiKeys) after SecretStore is ready
if (chatRoom.llmManager) {
    chatRoom.llmManager._loadSecrets().then(function () {
        console.log('[main] LLM secrets loaded');
    }).catch(function (e) {
        console.warn('[main] Failed to load LLM secrets: ' + e.message);
    });
}

