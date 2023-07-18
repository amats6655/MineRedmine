$(document).ready(function (){
    $(window).on("beforeunload", function (){
        $("#c").show();
        setTimeout(function () {
            $("#c").fadeOut('slow');
        }, 5000);
    });
});

$(window).on("load", function (){
    $("#c").fadeOut('slow');
});


var linkLength = 10, initCount = 12;
var createEvery = 10; // milliseconds
var growTime = 3000; // ms too
var accuracy = 3; // Repeat the "constraints solving" part this many times per frame (reduce if slow)
var repulsionRadius = 15, repulsionForce = 50; // Fun with 100,200 (keep radius > linkLength to avoid overlappings)
var outRad = 100;
var growFactor = 2, backFactor = 3; // >= 1
var showFps = false;

var c = document.getElementById('c'),
    ctx = c.getContext('2d');

// Background
(function(){
    var bg = document.createElement('canvas'),
        ctx = bg.getContext('2d');
    bg.width = 1; bg.height = 7;
    var gradient = ctx.createLinearGradient(0,0,0,bg.height);
    var colors = ['#289c8c', '#289790'];
    gradient.addColorStop(0, colors[0]);
    gradient.addColorStop(.5,colors[1]);
    gradient.addColorStop(1, colors[0]);
    ctx.fillStyle = gradient;
    ctx.fillRect(0,0,bg.width,bg.height);
    c.style.background = 'url('+bg.toDataURL()+')';
})();

(window.onresize = function() {
    c.width = c.offsetWidth;
    c.height = c.offsetHeight;
    ctx.strokeStyle = 'rgba(255,255,255,1)';
    ctx.lineWidth = 2.2;
    ctx.lineJoin = 'bevel';
    ctx.lineCap = 'round';
    // Glow (can be slow)
    ctx.shadowColor = 'rgba(255,255,255,1)';
    ctx.shadowBlur = 15;
    ctx.textBaseline = 'top';
})();

function comeCloser(n, goal, factor, limit)
{
    return (limit && Math.abs(goal - n) < limit) ? goal : n + (goal - n) / (factor || 10);
}
function Point(x, y, init)
{
    this._x = this.x = x;
    this._y = this.y = y;
}
function Link(p1, p2, length)
{
    this.p1 = p1;
    this.p2 = p2;
    this._length = this.length = this.goal = length;
    p1.next = p2;
}
var points = [], links = [];
var init = function()
{
    points = []; links = [];
    for(var i = 0; i < initCount; i++)
    {
        points.push(new Point(0, linkLength * (i - initCount * .5 + .5)));
        if(i)
            links.push(new Link(points[i-1], points[i], linkLength));
    }
};
init();

function draw()
{
    ctx.clearRect(0,0,c.width,c.height);
    ctx.save();
    ctx.translate(c.width * .5, c.height * .5);
    ctx.beginPath();
    var p = points[0];
    ctx.moveTo(p.x, p.y);
    while((p = p.next))
        ctx.lineTo(p.x, p.y);
    ctx.stroke();
    ctx.restore();
}

function normVector(v, d)
{
    if(d === undefined)
        d = Math.sqrt(v[0] * v[0] + v[1] * v[1]);
    if(d < 1e-6)
    {
        var a = Math.random() * 2 * Math.PI;
        return [Math.cos(a), Math.sin(a)];
    }
    return [v[0] / d, v[1] / d];
}

var counter = 0, total = 0;
var sqOutRad = outRad * outRad, sqRepRad = repulsionRadius * repulsionRadius;
var retracting = false;
function compute(dt)
{
    if(dt > 100) dt = 100;
    if(retracting)
    {
        var done = true;
        for(var i = points.length; i--;)
        {
            var p = points[i], src = p.source || p;
            p.x = comeCloser(p.x, src._x, backFactor, .001);
            p.y = comeCloser(p.y, src._y, backFactor, .001);
            if(p.x != src._x || p.y != src._y) done = false;
        }
        if(done)
        {
            counter = total = 0;
            init();
            retracting = false;
        }
        return;
    }
    total += dt;
    if(total > growTime)
        retracting = true;
    counter += dt;
    while(counter > createEvery) // Insert new point
    {
        var link = links[~~(Math.random() * (links.length - 1) + 1)];
        var p = link.p1;
        var np = new Point(p.x + Math.random() * .001, p.y + Math.random() * .001);
        np.source = p.source || p;
        link.p1 = np;
        np.next = link.p2;
        var nl = new Link(p, np, 0);
        //nl.goal = Math.random() * 4 + 8;
        nl.goal = linkLength;
        points.push(np);
        links.push(nl);
        counter -= createEvery;
    }
    for(var i = links.length; i--;)
        links[i].length = comeCloser(links[i].length, links[i].goal, growFactor);
    for(var s = 0; s < accuracy; s++)
    {
        for(var i = 0, l = points.length; i < l; i++)
        {
            var a = points[i];
            var sqcd = a.x * a.x + a.y * a.y;
            if(sqcd > sqOutRad) // Force the thing to stay inside a disc
            {
                var cd = Math.sqrt(sqcd);
                var f = cd - outRad;
                if(f < 0) f = 0;
                f = 1 - f * .1 / cd;
                a.x *= f;
                a.y *= f;
            }
            for(var j = i + 1; j < l; j++) // Point-point repulsion
            {
                var b = points[j];
                if(a.next == b || b.next == a) continue;
                var d = [b.x - a.x, b.y - a.y], sqd = d[0] * d[0] + d[1] * d[1];
                if(sqd > sqRepRad) continue;
                d = normVector(d, Math.sqrt(sqd));
                var f = repulsionForce / (sqd + 1);
                a.x -= f * d[0];
                a.y -= f * d[1];
                b.x += f * d[0];
                b.y += f * d[1];
            }
        }
        for(var i = links.length; i--;) // Force links distance
        {
            var link = links[i], from = link.p1, to = link.p2;
            var dist = [to.x - from.x, to.y - from.y], l = Math.sqrt(dist[0] * dist[0] + dist[1] * dist[1]);
            dist = normVector(dist, l);
            var delta = (l - link.length) * .5;
            from.x += delta * dist[0];
            from.y += delta * dist[1];
            to.x -= delta * dist[0];
            to.y -= delta * dist[1];
        }
    }
}

var pt, fps = 0, frameCount = 0, prevFps, fpsInterval = 100;
function loop(t)
{
    if(!pt) pt = t;
    var dt = t - pt;
    pt = t;
    compute(dt);
    draw();
    if(showFps)
    {
        if(!prevFps) prevFps = t;
        frameCount++;
        if(t - prevFps >= fpsInterval)
        {
            fps = 1000 * frameCount / (t - prevFps);
            frameCount = 0;
            prevFps = t;
        }
        ctx.fillText(fps, 0, 0);
    }
    requestAnimationFrame(loop);
}
requestAnimationFrame(loop);
