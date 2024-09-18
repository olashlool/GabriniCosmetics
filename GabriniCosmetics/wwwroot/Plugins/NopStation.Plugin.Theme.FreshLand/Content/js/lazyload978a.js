/*!
 * Lazy Load - JavaScript plugin for lazy loading images
 *
 * Copyright (c) 2007-2019 Mika Tuupola
 *
 * Licensed under the MIT license:
 *   http://www.opensource.org/licenses/mit-license.php
 *
 * Project home:
 *   https://appelsiini.net/projects/lazyload
 *
 * Version: 2.0.0-rc.2
 *
 */
(function(n,t){typeof exports=="object"?module.exports=t(n):typeof define=="function"&&define.amd?define([],t):n.LazyLoad=t(n)})(typeof global!="undefined"?global:this.window||this.global,function(n){"use strict";function t(n,t){this.settings=i(r,t||{});this.images=n||document.querySelectorAll(this.settings.selector);this.observer=null;this.init()}typeof define=="function"&&define.amd&&(n=window);const r={src:"data-src",srcset:"data-srcset",selector:".lazyload",root:null,rootMargin:"0px",threshold:0},i=function(){let n={},r=!1,t=0,u=arguments.length;Object.prototype.toString.call(arguments[0])==="[object Boolean]"&&(r=arguments[0],t++);let f=function(t){for(let u in t)Object.prototype.hasOwnProperty.call(t,u)&&(n[u]=r&&Object.prototype.toString.call(t[u])==="[object Object]"?i(!0,n[u],t[u]):t[u])};for(;t<u;t++){let n=arguments[t];f(n)}return n};if(t.prototype={init:function(){if(!n.IntersectionObserver){this.loadImages();return}let t=this,i={root:this.settings.root,rootMargin:this.settings.rootMargin,threshold:[this.settings.threshold]};this.observer=new IntersectionObserver(function(n){Array.prototype.forEach.call(n,function(n){if(n.isIntersecting){t.observer.unobserve(n.target);let i=n.target.getAttribute(t.settings.src),r=n.target.getAttribute(t.settings.srcset);"img"===n.target.tagName.toLowerCase()?(i&&(n.target.src=i),r&&(n.target.srcset=r)):n.target.style.backgroundImage="url("+i+")"}})},i);Array.prototype.forEach.call(this.images,function(n){t.observer.observe(n)})},loadAndDestroy:function(){this.settings&&(this.loadImages(),this.destroy())},loadImages:function(){if(this.settings){let n=this;Array.prototype.forEach.call(this.images,function(t){let i=t.getAttribute(n.settings.src),r=t.getAttribute(n.settings.srcset);"img"===t.tagName.toLowerCase()?(i&&(t.src=i),r&&(t.srcset=r)):t.style.backgroundImage="url('"+i+"')"})}},destroy:function(){this.settings&&(this.observer.disconnect(),this.settings=null)}},n.lazyload=function(n,i){return new t(n,i)},n.jQuery){const i=n.jQuery;i.fn.lazyload=function(n){return n=n||{},n.attribute=n.attribute||"data-src",new t(i.makeArray(this),n),this}}return t});
