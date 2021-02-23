const webpack = require('webpack');
const path = require('path');

module.exports = {
    entry: {
        player: './Scripts/Player.ts'
    },
    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, 'wwwroot')
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            }
        ]
    },
    resolve: {
        extensions: ['.txs', '.ts', '.js']
    }
};