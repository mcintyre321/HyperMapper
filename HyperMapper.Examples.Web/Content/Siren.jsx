
class SirenField extends React.Component {
    constructor(props) {
        super(props);
        this.state = props;
    }
  
    render() {
        var id = Math.random().toString(36);
        return (
            <div>
                <label htmlFor={id}>{this.state.title || this.state.name}</label>
                <input id={id} type={this.state.type} name={this.state.name} def={this.state.value || ""} />
            </div>
        );
    }
}

class SirenAction extends React.Component {
    constructor(props) {
        super(props);
        this.state = props;

    }
    render() {

        return (
            <form action={this.state.href} method={this.state.method || "POST"}>
                {
                (this.state.fields || []).map(function(e) {
                return <SirenField key={e.name} {...e} />;
                })
                }
                <input value={this.state.name} type="submit" />
            </form>
        );

    }
}
class SirenLink extends React.Component {
    constructor(props) {
        super(props);
        this.state = props;

    }
    render() {
        return <a href={this.state.href}>{this.state.title || this.state.href}</a>;
    }
}

class Siren extends React.Component {
    constructor(props) {
        super(props);
        this.state = props;

    }
    render() {
        var state = this.state;
        return (
            <div className="entity">
            {
                Object.keys(state.properties || []).map(function(key) {
                    return <div UrlPart={key}>{key}: {state.properties[key]}</div>;
                })
            }
            {
                (state.entities || []).map(function(e) {
                    if (e.href) {
                        return <Siren key={e.href} {...e} />;
                    } else {
                        var self = e.links.find(function(link) { return link.rel.find(function(rel) { return rel === "self"; }) });

                        return <Siren key={self.href} {...e} />;
                    }
                })
            }
                {
                (state.actions || []).map(function(e) {
                return <SirenAction key={e.name || e.href} {...e} />;
                })
                }
            {
                (state.links || []).map(function(e) {
                    return <div><SirenLink key={e.name || e.href} {...e} /></div>;
                })
            } 

            </div>
        );
    }
}