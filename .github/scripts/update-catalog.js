const fs = require('fs');

const widgetId = process.env.WIDGET_ID;
const version = process.env.VERSION;
const downloadUrl = process.env.DOWNLOAD_URL;
const fileSize = parseInt(process.env.FILE_SIZE || '0', 10);
const sha256 = process.env.SHA256;
const changelog = process.env.CHANGELOG || `Release v${version}`;

if (!widgetId || !version || !downloadUrl) {
  console.error('Missing required params');
  process.exit(1);
}

const catalogPath = 'widgets.json';
const catalog = JSON.parse(fs.readFileSync(catalogPath, 'utf8'));

const index = catalog.widgets.findIndex(w => w.id === widgetId);

const widget = {
  id: widgetId,
  name: getWidgetName(widgetId),
  description: getWidgetDescription(widgetId),
  author: 'WDesk Team',
  version: version,
  previewEmoji: getWidgetEmoji(widgetId),
  category: getWidgetCategory(widgetId),
  tags: getWidgetTags(widgetId),
  downloadUrl: downloadUrl,
  fileSize: fileSize,
  sha256: sha256,
  minWDeskVersion: '1.0.0',
  releaseDate: new Date().toISOString().split('T')[0],
  changelog: changelog,
  official: true,
  featured: true,
  isPaid: false,
  price: 0,
  currency: 'USD',
  downloads: 0,
  rating: 5.0
};

if (index >= 0) {
  Object.assign(catalog.widgets[index], widget);
  console.log(`Updated ${widgetId} to v${version}`);
} else {
  catalog.widgets.push(widget);
  console.log(`Added ${widgetId} v${version}`);
}

catalog.updatedAt = new Date().toISOString();
fs.writeFileSync(catalogPath, JSON.stringify(catalog, null, 2));

function getWidgetName(id) {
  const names = {
    prayer: 'Prayer Times',
    clock: 'Clock',
    weather: 'Weather'
  };
  return names[id] || id;
}

function getWidgetDescription(id) {
  const descs = {
    prayer: 'Daily prayer times with next-prayer countdown',
    clock: 'Digital and analog clock widget',
    weather: 'Real-time weather information'
  };
  return descs[id] || 'WDesk Widget';
}

function getWidgetEmoji(id) {
  const emojis = { prayer: '🕌', clock: '🕐', weather: '☁️' };
  return emojis[id] || '🎨';
}

function getWidgetCategory(id) {
  const cats = { prayer: 'Islamic', clock: 'Time', weather: 'Weather' };
  return cats[id] || 'Custom';
}

function getWidgetTags(id) {
  const tags = {
    prayer: ['prayer', 'islamic', 'time'],
    clock: ['clock', 'time'],
    weather: ['weather', 'forecast']
  };
  return tags[id] || [];
}